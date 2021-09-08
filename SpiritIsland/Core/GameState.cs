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
			Tokens = new Tokens_ForIsland( this );

			TimePassed += PreRavaging.EndOfRound;
			TimePassed += PreBuilding.EndOfRound;
		}

	// == Components ==
	public Fear Fear { get; }
		public InvaderDeck InvaderDeck { get; set; }
		public Island Island { get; set; }
		public Spirit[] Spirits { get; }
		public Tokens_ForIsland Tokens { get; }
		public PowerCardDeck MajorCards {get; set; }
		public PowerCardDeck MinorCards { get; set; }
		// Branch & Claw
		public void SkipAllInvaderActions( Space target ) {
			ModifyRavage(target, cfg=>cfg.ShouldRavage=false );
			SkipBuild( target );
			skipExplore.Add(target);
		}

		public void SkipExplore( params Space[] target ) {
			skipExplore.AddRange( target );
		}

		readonly List<Space> skipExplore = new List<Space>();

		public virtual void Initialize() {

			foreach(var board in Island.Boards)
				foreach(var space in board.Spaces)
					space.InitTokens( Tokens[space] );

			Task.WaitAll( Explore( InvaderDeck.Explore ) );
			InvaderDeck.Advance();
			InitSpirits();

			BlightCard.OnGameStart( this );
		}

		// == EVENTS ==
		public AsyncEvent<Space[]> PreRavaging = new AsyncEvent<Space[]>();						// A Spread of Rampant Green - stop ravage
		public AsyncEvent<BuildingEventArgs> PreBuilding = new AsyncEvent<BuildingEventArgs>();						// A Spread of Rampant Green - stop build
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

		#region Blight

		public int blightOnCard; // 2 per player
		public IBlightCard BlightCard = new NullBlightCard();

		/// <summary>Causes cascading</summary>
		public async Task BlightLand( Space blightSpace ){

			while(blightSpace != null) {

				var tokens = Tokens[blightSpace];
				bool isFirstBlight = tokens.Blight.Count == 0;

				foreach(var spirit in Spirits)
					if(spirit.Presence.IsOn( blightSpace ))
						spirit.Presence.Destroy( blightSpace );

				tokens.Blight.Count++;
				--blightOnCard;
				if(BlightCard != null && blightOnCard == 0)
					BlightCard.OnBlightDepleated( this );

				blightSpace = isFirstBlight ? null
					: await Spirits[0].Action.Decide(new SelectAdjacentDecision(
						"Cascade blight to", 
						blightSpace, 
						GatherPush.None,
						blightSpace.Adjacent.Where( x => SpaceFilter.ForCascadingBlight.TerrainMapper( x ) != Terrain.Ocean ),
						Present.Always
					));
			}

		}

		public void AddBlight( Space space, int delta=1 ){ // also used for removing blight
			var counts = Tokens[space];
			int newCount = counts.Blight + delta;
			if(newCount<0) return;
			counts.Blight.Count = newCount;
			blightOnCard -= delta;
		}

		public bool HasBlight( Space s ) => GetBlightOnSpace(s) > 0;

		public int GetBlightOnSpace( Space space ){ return Tokens[space].Blight; }

		#endregion

		#region Invaders

		public int GetDefence( Space space ) => Tokens[space].Defend;

		public void Defend( Space space, int delta ) {
			Tokens[space].Defend.Count += delta;
		}

		public ConfigureRavage GetRavageConfiguration( Space space ) => _ravageConfig.ContainsKey( space ) ? _ravageConfig[space] : new ConfigureRavage();

		public void ModifyRavage( Space space, Action<ConfigureRavage> action ) {
			if(!_ravageConfig.ContainsKey( space ))
				_ravageConfig.Add( space, new ConfigureRavage() );
			action( _ravageConfig[space] );
		}

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

		public void SkipRavage( params Space[] spaces ) {
			foreach(var space in spaces)
				ModifyRavage( space, cfg => cfg.ShouldRavage = false );
		}

		readonly Dictionary<Space, ConfigureRavage> _ravageConfig = new Dictionary<Space, ConfigureRavage>(); // change ravage state of a Space

		protected virtual async Task<string> RavageSpace( InvaderGroup grp ) {
			var cfg = GetRavageConfiguration( grp.Space );
			var eng = new RavageEngine( this, grp, cfg );
			await eng.Exec();
			return grp.Space.Label + ": " + eng.log.Join( "  " );
		}

		public async Task<string[]> Build( InvaderCard invaderCard ) {

			// Build normal
			var buildLands = Island.Boards.SelectMany( board => board.Spaces )
				.Where( invaderCard.Matches )
				.GroupBy( s=>s )
				.ToDictionary( grp=>grp.Key, grp=>grp.Count() );

			var args = new BuildingEventArgs {
				 BuildTypes = new Dictionary<Space, BuildingEventArgs.BuildType>(),
				 Spaces = new CountDictionary<Space>( buildLands ),
			};

			await PreBuilding?.InvokeAsync(this,args);

			return args.Spaces
				.Where( pair=>pair.Value>=0 ) // in case we end up with any negative counts
				.Select( pair => Tokens[pair.Key] )
				.Where( tokens => tokens.HasInvaders() )
				.Select( tokens => tokens.Space.Label + " gets " + Build( tokens, args.BuildTypes.ContainsKey(tokens.Space) ? args.BuildTypes[tokens.Space] : BuildingEventArgs.BuildType.TownsAndCities ) )
				.ToArray();
		}

		public void SkipBuild( params Space[] target ) {
			PreBuilding.ForRound.Add( (GameState gs, BuildingEventArgs args) => {
				foreach(var skip in target)
					args.Spaces[skip]--;
				return Task.CompletedTask;
			});
		}

		protected virtual async Task<string> Build( TokenCountDictionary counts, BuildingEventArgs.BuildType buildType ) {
			// Determine type to build
			int townCount = counts.Sum( Invader.Town );
			int cityCount = counts.Sum( Invader.City );
			TokenGroup invaderToAdd = townCount > cityCount ? Invader.City : Invader.Town;

			// check if we should
			bool shouldBuild = buildType switch {
				BuildingEventArgs.BuildType.CitiesOnly => invaderToAdd == Invader.City,
				BuildingEventArgs.BuildType.TownsOnly => invaderToAdd == Invader.Town,
				_ => true,
			};
			// build it
			if(shouldBuild)
				await Tokens.Add( invaderToAdd, counts.Space, 1 );

			return invaderToAdd.Label;
		}

		public async Task<Space[]> Explore( InvaderCard invaderCard ) {
			bool HasTownOrCity( Space space ) {
				return Tokens[ space ].HasAny(Invader.Town,Invader.City);
			}
			var bob = Island.Boards.SelectMany( board => board.Spaces )
				.Where( invaderCard.Matches )
				.Where( space => space.IsCostal || space.Range( 1 ).Any( HasTownOrCity ) )
				.Except( skipExplore )
				.ToArray();
			var spaces = new List<Space>();
			foreach(var b in bob)
				if( await ExploresSpace( b ) )
					spaces.Add( b );
			return spaces.ToArray();
		}

		protected virtual async Task<bool> ExploresSpace(Space space ) {
			await Tokens.Add( Invader.Explorer, space );
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

		public async Task SpiritFree_FearCard_DamageInvaders( Space space, int damage ) {
			if(damage == 0) return;
			await Invaders.On( space, Cause.Fear ).ApplySmartDamageToGroup( damage );
		}

		#endregion

	}


	public class AsyncEvent<T> {

		public async Task InvokeAsync(GameState gameState,T t) {
			foreach(var handler in ForRound)
				await TryHandle( handler, gameState, t );
			foreach(var handler in ForGame)
				await TryHandle( handler, gameState, t );
		}

		static async Task TryHandle( Func<GameState, T, Task> handler, GameState gameState, T t ) {
			try {
				await handler( gameState, t );
			}
			catch(Exception) {
			}
		}
		public void EndOfRound(GameState _) { ForRound.Clear(); }
		public List<Func<GameState,T,Task>> ForRound = new List<Func<GameState, T,Task>>();
		public List<Func<GameState, T, Task>> ForGame = new List<Func<GameState, T, Task>>();
	}

	public class SyncEvent<T> {
		public void Invoke( GameState gameState, T t ) {
			foreach(var handler in ForRound)
				handler( gameState, t );
			foreach(var handler in ForGame)
				handler( gameState, t );
		}
		public void EndOfRound(GameState _) { ForRound.Clear(); }
		public List<Action<GameState, T>> ForRound = new List<Action<GameState, T>>();
		public List<Action<GameState, T>> ForGame = new List<Action<GameState, T>>();
	}

	public class BuildingEventArgs {
		public CountDictionary<Space> Spaces;
		public Dictionary<Space,BuildType> BuildTypes;
		public enum BuildType { TownsAndCities, TownsOnly, CitiesOnly }
	}

}
