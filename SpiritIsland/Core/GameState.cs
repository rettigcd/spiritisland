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
			Dahan = new Dahan(this);
			Fear = new Fear( this );
		}

		// == Components ==
		public Fear Fear { get; }
		public Dahan Dahan { get; }
		public InvaderDeck InvaderDeck { get; set; }
		public Island Island { get; set; }
		public Spirit[] Spirits { get; }
		public PowerCardDeck MajorCards {get; set; }
		public PowerCardDeck MinorCards { get; set; }
		// Branch & Claw
		public TokenCounts Beasts { get; set; } = new TokenCounts();
		public TokenCounts Wilds { get; set; } = new TokenCounts();

		internal void SkipAllInvaderActions( Space target ) {
			ModRavage(target, cfg=>cfg.ShouldRavage=false );
			skipBuild.Add( target );
			skipExplore.Add(target);
		}

		internal void SkipRavage( params Space[] spaces ) {
			foreach(var space in spaces )
				ModRavage(space, cfg=>cfg.ShouldRavage=false );
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
					InitSpace( space );

			Explore( InvaderDeck.Explore );
			InvaderDeck.Advance();
			InitSpirits();

			BlightCard.OnGameStart( this );
		}

		// == EVENTS ==
		public AsyncEvent<Space[]> PreRavaging = new AsyncEvent<Space[]>();						// A Spread of Rampant Green - stop ravage
		public AsyncEvent<Space[]> PreBuilding = new AsyncEvent<Space[]>();						// A Spread of Rampant Green - stop build
		public AsyncEvent<InvaderMovedArgs> InvaderMoved = new AsyncEvent<InvaderMovedArgs>();                    // Thunderspeaker
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
			// heal
			foreach(var pair in invaderCount)
				new InvaderGroup( pair.Key, pair.Value, null, Cause.None ).Heal();

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

		void InitSpace( Space space ) {
			var counts = space.StartUpCounts;
			Adjust( space, InvaderSpecific.City, counts.Cities );
			Adjust( space, InvaderSpecific.Town, counts.Towns );
			Adjust( space, InvaderSpecific.Explorer, counts.Explorers );
			this.Dahan.Adjust( space, counts.Dahan );
			blightCount[space] += counts.Blight; // don't use AddBlight because that pulls it from the card and triggers blighted island
		}

		public void Defend( Space space, int delta ) {
			ModRavage(space, cfg=>cfg.Defend += delta);
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

		public void AddBlight( Space space, int delta=1 ){
			int newCount = blightCount[space] + delta;
			if(newCount<0) return;
			blightCount[space] = newCount;
			blightOnCard -= delta;
		}

		public bool HasBlight( Space s ) => blightCount[s] > 0;
		public int GetBlightOnSpace( Space space ){ return blightCount[space]; }

		public Stack<Space> cascadingBlight = new Stack<Space>();

		#endregion

		#region Invaders

		public int GetDefence( Space space ) => GetRavageConfiguration( space ).Defend;

		public ConfigureRavage GetRavageConfiguration( Space space ) => _ravageConfig.ContainsKey( space ) ? _ravageConfig[space] : new ConfigureRavage();

		public void ModRavage( Space space, Action<ConfigureRavage> action ) {
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

			var ravageGroups = ravageSpaces
				.Select( InvadersOn )
				.Where( group => group.InvaderTypesPresent_Specific.Any() )
				.Cast<InvaderGroup>()
				.ToArray();

			var msgs = new List<string>();
			foreach(var grp in ravageGroups) {
				var eng = new RavageEngine( this, grp, GetRavageConfiguration(grp.Space) );
				await eng.Exec();
				msgs.Add( grp.Space.Label + ": " + eng.log.Join( "  " ) );
			}
			return msgs.ToArray();
		}

		public async Task<string[]> Build( InvaderCard invaderCard ) {

			var buildLands = Island.Boards.SelectMany( board => board.Spaces )
				.Where( invaderCard.Matches )
				.Except( skipBuild )
				.ToArray();

			await PreBuilding?.InvokeAsync(this,buildLands);

			buildLands = buildLands.Except( skipBuild ).ToArray(); // reload in case they changed

			return buildLands
				.Select( InvadersOn )
				.Cast<InvaderGroup>()
				.Where( group => group.InvaderTypesPresent_Specific.Any() )
				.Select( Build )
				.ToArray();
		}

		public Space[] Explore( InvaderCard invaderCard ) {
			bool HasTownOrCity( Space space ) {
				var invaders = InvadersOn( space );
				return invaders.HasTown || invaders.HasCity;
			}
			var exploredSpaces = Island.Boards.SelectMany( board => board.Spaces )
				.Where( invaderCard.Matches )
				.Where( space => space.IsCostal
						 || space.Range( 1 ).Any( HasTownOrCity )
				)
				.Except( skipExplore )
				.ToArray();

			foreach(var space in exploredSpaces)
				Adjust( space, InvaderSpecific.Explorer, 1 );
			return exploredSpaces;
		}

		public bool HasInvaders( Space space ) 
			=> GetCounts(space).Any(x=>x>0);

		public void Adjust(Space space, InvaderSpecific invader, int count)
			=> ((InvaderGroup)InvadersOn(space)).Adjust(invader,count);
		
		public Task MoveInvader(InvaderSpecific invader, Space from, Space to ) {
			Adjust( from,invader,-1);
			Adjust( to, invader, 1 );
			return InvaderMoved.InvokeAsync( this, new InvaderMovedArgs { from = from, to = to, Invader = invader } );
		}

		public InvaderGroup_Readonly InvadersOn(Space targetSpace) {
			return new InvaderGroup( targetSpace, this.GetCounts( targetSpace ), Fear.AddDirect, Cause.Ravage );
		}

		//public InvaderGroup AttackInvadersOn( Space targetSpace, Func<GameState, Space, int[],InvaderGroup> factory ) {
		//	return factory( this, targetSpace, this.GetCounts( targetSpace ) );
		//}

		string Build( InvaderGroup group ) {
			int townCount = group[InvaderSpecific.Town] + group[InvaderSpecific.Town1];
			int cityCount = group[InvaderSpecific.City] + group[InvaderSpecific.City2] + group[InvaderSpecific.City1];
			var invaderToAdd = townCount > cityCount ? InvaderSpecific.City : InvaderSpecific.Town;
			Adjust( group.Space, invaderToAdd, 1 );
			return $"{group.Space.Label} gets {invaderToAdd.Generic.Label}";
		}


		public async Task SpiritFree_DamageInvaders(Space space,int damage){ // !!! let players choose the item to apply damage to
			if(damage==0) return;
			await ((InvaderGroup)InvadersOn(space)).ApplySmartDamageToGroup( damage );
		}

		#endregion

		readonly CountDictionary<Space> blightCount = new CountDictionary<Space>();

		readonly Dictionary<Space,int[]> invaderCount = new Dictionary<Space,int[]>();

		public int[] GetCounts(Space space) {
			if(invaderCount.ContainsKey(space)) return invaderCount[space];
			return invaderCount[space] = new int[InvaderSpecific.TypesCount+1]; // 1 for the total
		}

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

	public class TokenCounts {
		public void AddTo( Space space ) { count[space]++; }
		public bool AreOn( Space s ) => count[s] > 0;
		readonly CountDictionary<Space> count = new CountDictionary<Space>();
	}

}
