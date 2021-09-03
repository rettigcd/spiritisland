using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland {

	public class GameState {

		// base-1,  game starts in round-1
		public int Round { get; private set; }

		/// <summary>
		/// Simplified constructor for single-player
		/// </summary>
		public GameState( Spirit spirit, Board board ) : this(spirit) {
			this.Island = new Island(board);
		}

		public GameState(params Spirit[] spirits){
			if(spirits.Length==0) throw new ArgumentException("Game must include at least 1 spirit");
			this.Spirits = spirits;
			InvaderDeck = new InvaderDeck();
			Round = 1;
			Fear = new Fear( this );
			Invaders = new Invaders( this );
			Tokens = new IslandTokens( this );
		}

		// == Components ==
		public Fear Fear { get; }
		public InvaderDeck InvaderDeck { get; set; }
		public Island Island { get; set; }
		public Spirit[] Spirits { get; }
		public IslandTokens Tokens { get; }
		public PowerCardDeck MajorCards {get; set; }
		public PowerCardDeck MinorCards { get; set; }
		// Branch & Claw
		internal void SkipAllInvaderActions( Space target ) {
			ModifyRavage(target, cfg=>cfg.ShouldRavage=false );
			skipBuild.Add( target );
			skipExplore.Add(target);
		}

		internal void SkipRavage( params Space[] spaces ) {
			foreach(var space in spaces )
				ModifyRavage(space, cfg=>cfg.ShouldRavage=false );
		}

		internal void SkipBuild( params Space[] target ) {
			skipBuild.AddRange( target );
		}

		internal void SkipExplore( params Space[] target ) {
			skipExplore.AddRange( target );
		}

		readonly List<Space> skipBuild = new List<Space>();
		readonly List<Space> skipExplore = new List<Space>();

		public void Initialize() {

			foreach(var board in Island.Boards)
				foreach(var space in board.Spaces)
					space.InitTokens( Tokens[space] );

			Explore( InvaderDeck.Explore );
			InvaderDeck.Advance();
			InitSpirits();

			BlightCard.OnGameStart( this );
		}

		// == EVENTS ==
		public AsyncEvent<Space[]> PreRavaging = new AsyncEvent<Space[]>();						// A Spread of Rampant Green - stop ravage
		public AsyncEvent<Space[]> PreBuilding = new AsyncEvent<Space[]>();						// A Spread of Rampant Green - stop build
		public event Action<GameState> TimePassed;												// Spirit cleanup
		// == Single Round hooks
		public Stack<Func<GameState, Task>> TimePasses_ThisRound = new Stack<Func<GameState, Task>>();        // Gift of Power

		void InitSpirits() {
			if(Spirits.Length != Island.Boards.Length)
				throw new InvalidOperationException( "# of spirits and islands must match" );
			for(int i = 0; i < Spirits.Length; ++i)
				Spirits[i].Initialize( Island.Boards[i], this );
		}

		public async Task TimePasses() {

			await ExecuteAndClear_OneRoundEvents(); // this is async because of Gift of Contancy has user action 'at end of turn'

			TimePassed?.Invoke( this );
			++Round;
		}

		async Task ExecuteAndClear_OneRoundEvents() {
			_ravageConfig.Clear();

			// stack allows us to unwind items in reverse order from when we set them up
			while(TimePasses_ThisRound.Count > 0) 
				await TimePasses_ThisRound.Pop()( this );
		}

		public void Defend( Space space, int delta ) {
			ModifyRavage(space, cfg=>cfg.Defend += delta);
		}

		#region Blight

		public int blightOnCard; // 2 per player
		public IBlightCard BlightCard = new NullBlightCard();

		/// <summary>Causes cascading</summary>
		public Task BlightLand( Space space ){
			if(HasBlight(space))
				cascadingBlight.Push(space); // !!! resolve during battle
			AddBlight(space);

			foreach(var spirit in Spirits)
				if(spirit.Presence.IsOn(space))
					spirit.Presence.Destroy(space);

			--blightOnCard;
			if(BlightCard != null && blightOnCard==0)
				BlightCard.OnBlightDepleated(this);
			return Task.CompletedTask;
		}

		public void AddBlight( Space space, int delta=1 ){ // also used for removing blight
			var counts = Tokens[space];
			int newCount = counts[TokenType.Blight] + delta;
			if(newCount<0) return;
			counts[TokenType.Blight] = newCount;
			blightOnCard -= delta;
		}

		public bool HasBlight( Space s ) => GetBlightOnSpace(s) > 0;
		public int GetBlightOnSpace( Space space ){ return Tokens[space][TokenType.Blight]; }

		public Stack<Space> cascadingBlight = new Stack<Space>();

		#endregion

		#region Invaders

		public int GetDefence( Space space ) => GetRavageConfiguration( space ).Defend;

		public ConfigureRavage GetRavageConfiguration( Space space ) => _ravageConfig.ContainsKey( space ) ? _ravageConfig[space] : new ConfigureRavage();

		public void ModifyRavage( Space space, Action<ConfigureRavage> action ) {
			if(!_ravageConfig.ContainsKey( space ))
				_ravageConfig.Add( space, new ConfigureRavage() );
			action( _ravageConfig[space] );
		}

		readonly Dictionary<Space, ConfigureRavage> _ravageConfig = new Dictionary<Space, ConfigureRavage>(); // change ravage state of a Space

		public async Task<string[]> Ravage( InvaderCard invaderCard ) {
			if(invaderCard == null) return Array.Empty<string>();

			var initialRavageSpaces = Island.Boards.SelectMany( board => board.Spaces )
				.Where( invaderCard.Matches )
				.Where( x=>GetRavageConfiguration(x).ShouldRavage )
				.ToArray();

			await PreRavaging?.InvokeAsync( this, initialRavageSpaces );

			var ravageSpaces = initialRavageSpaces
				.Where( x => GetRavageConfiguration( x ).ShouldRavage ) // not sure this is necessary since it is called during execute
				.ToArray();

			var xx = ravageSpaces
				.Select( x => Invaders.On( x, Cause.Ravage ) )
				.ToList();

			var ravageGroups = xx
				.Where( group => group.Counts.HasInvaders() )
				.Cast<InvaderGroup>()
				.ToArray();

			var msgs = new List<string>();
			foreach(var grp in ravageGroups)
				msgs.Add(await RavageSpace( grp ) );
			return msgs.ToArray();
		}

		protected virtual async Task<string> RavageSpace( InvaderGroup grp ) {
			var cfg = GetRavageConfiguration( grp.Space );
			var eng = new RavageEngine( this, grp, cfg );
			await eng.Exec();
			return grp.Space.Label + ": " + eng.log.Join( "  " );
		}

		public async Task<string[]> Build( InvaderCard invaderCard ) {

			var buildLands = Island.Boards.SelectMany( board => board.Spaces )
				.Where( invaderCard.Matches )
				.Except( skipBuild )
				.ToArray();

			await PreBuilding?.InvokeAsync(this,buildLands);

			buildLands = buildLands.Except( skipBuild ).ToArray(); // reload in case they changed

			return buildLands
				.Select( x=>new {space=x, cts=Tokens[x]} ) // !!! store space in Counts
				.Where( tup => tup.cts.HasInvaders() )
				.Select( tup => tup.space.Label + " gets " + Build( tup.space, tup.cts ) )
				.ToArray();
		}

		protected virtual string Build( Space space, TokenCountDictionary counts ) {
			int townCount = counts.Sum( Invader.Town );
			int cityCount = counts.Sum( Invader.City );
			Token invaderToAdd = townCount > cityCount ? Invader.City[3] : Invader.Town[2];
			counts.Adjust( invaderToAdd, 1 );
			return invaderToAdd.Generic.Label;
		}

		public Space[] Explore( InvaderCard invaderCard ) {
			bool HasTownOrCity( Space space ) {
				return Tokens[ space ].HasAny(Invader.Town,Invader.City);
			}
			return Island.Boards.SelectMany( board => board.Spaces )
				.Where( invaderCard.Matches )
				.Where( space => space.IsCostal || space.Range( 1 ).Any( HasTownOrCity ) )
				.Except( skipExplore )
				.Where( ExploresSpace )
				.ToArray();
		}

		protected virtual bool ExploresSpace(Space space ) {
			Tokens[space].Adjust( Invader.Explorer[1], 1 );
			return true;
		}

		public Invaders Invaders { get; }

		#endregion

		#region Dahan Helpers

		public bool DahanIsOn( Space space ) => Tokens[space].Has( TokenType.Dahan );
		public int DahanGetCount( Space space ) =>Tokens[space].Sum( TokenType.Dahan );
		public void DahanAdjust( Space space, int delta = 1 ) => Tokens[space][TokenType.Dahan.Default] += delta;
		public Task DahanDestroy( Space space, int countToDestroy, Cause source ) {
			countToDestroy = Math.Min( countToDestroy, DahanGetCount( space ) );
			DahanAdjust( space, -countToDestroy );
			return Tokens.TokenDestroyed.InvokeAsync( this, new TokenDestroyedArgs {
				Token = TokenType.Dahan,
				space = space,
				count = countToDestroy,
				Source = source
			} );
		}

		#endregion

		#region Generic Token Helpers
		public Task Move( Token invader, Space from, Space to ) => Tokens.Move( invader, from, to );

		#endregion

		#region Invader Helpers

		public bool HasInvaders( Space space ) => Tokens[space].HasInvaders();

		public async Task SpiritFree_FearCard_DamageInvaders( Space space, int damage ) { // !!! let players choose the item to apply damage to
			if(damage == 0) return;
			await Invaders.On( space, Cause.Fear ).ApplySmartDamageToGroup( damage );
		}

		#endregion

	}


	public class AsyncEvent<T> {
		public async Task InvokeAsync(GameState gameState,T t) {
			foreach(var handler in Handlers)
				await TryHandle( handler, gameState, t );

		}

		static async Task TryHandle( Func<GameState, T, Task> handler, GameState gameState, T t ) {
			try {
				await handler( gameState, t );
			}
			catch(Exception) {
			}
		}

		public List<Func<GameState,T,Task>> Handlers = new List<Func<GameState, T,Task>>();
	}

	public class SyncEvent<T> {
		public void Invoke( GameState gameState, T t ) {
			foreach(var handler in Handlers)
				handler( gameState, t );
		}
		public List<Action<GameState, T>> Handlers = new List<Action<GameState, T>>();
	}


}
