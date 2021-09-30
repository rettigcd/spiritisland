using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland {

	public class GameState {

		// base-1,  game starts in round-1
		public int Round { get; private set; }

		#region constructors

		/// <summary>
		/// Simplified constructor for single-player
		/// </summary>
		public GameState( Spirit spirit, Board board ) : this(spirit) {
			this.Island = new Island(board);
		}

		public GameState(params Spirit[] spirits){
			if(spirits.Length==0) throw new ArgumentException("Game must include at least 1 spirit");
			this.Spirits = spirits;
			// Note: don't init invader deck here, let users substitute
			Round = 1;
			Fear = new Fear( this );
			Invaders = new Invaders( this );
			Tokens = new Tokens_ForIsland( this );

			TimePassed += PreRavaging.OnEndOfRound;
			TimePassed += PreBuilding.OnEndOfRound;
			TimePassed += PreExplore.OnEndOfRound;
		}

		#endregion

		// == Components ==
		public Fear Fear { get; }
		public Island Island { get; set; }
		public Spirit[] Spirits { get; }
		public Tokens_ForIsland Tokens { get; }
		public PowerCardDeck MajorCards {get; set; }
		public PowerCardDeck MinorCards { get; set; }

		public virtual void Initialize() {

			// Custom Deck not supplied, use default deck
			if(InvaderDeck == null)
				InvaderDeck = new InvaderDeck(new Random());

			foreach(var board in Island.Boards)
				foreach(var space in board.Spaces)
					space.InitTokens( Tokens[space] );

			// Do Explore - Advance card to the Build
//			deck.TurnOverExploreCards();
			Task.WaitAll( Explore( InvaderDeck.Explore[0] ) );
			InvaderDeck.Advance();

			InitRavageFromDeck();
			InitBuildFromDeck();
			InitSpirits();

			BlightCard.OnGameStart( this );
		}

		// == EVENTS ==
		public AsyncEvent<List<Space>> PreRavaging = new AsyncEvent<List<Space>>();				// A Spread of Rampant Green - stop ravage
		public AsyncEvent<BuildingEventArgs> PreBuilding = new AsyncEvent<BuildingEventArgs>();	// A Spread of Rampant Green - stop build
		public AsyncEvent<ExploreEventArgs> PreExplore = new AsyncEvent<ExploreEventArgs>();	       
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

			// Clear Defend
			foreach(var s in Island.AllSpaces)
				Tokens[s][TokenType.Defend] = 0;

			TimePassed?.Invoke( this );
			++Round;

			// Setup next round
			InitRavageFromDeck();
			InitBuildFromDeck();
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
					: await Spirits[0].Action.Decision(new Decision.AdjacentSpace(
						"Cascade blight to", 
						blightSpace, 
						Decision.GatherPush.None,
						blightSpace.Adjacent.Where( x => SpaceFilter.ForCascadingBlight.TerrainMapper( x ) != Terrain.Ocean ),
						Present.Always
					));
			}

		}

		/// <summary> Adds blight from the blight card to a space on the board. </summary>
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

		public int GetDefence( Space space ) => Tokens[space].Defend;

		public void Defend( Space space, int delta ) {
			Tokens[space].Defend.Count += delta;
		}

		public Invaders Invaders { get; }

		#region Invaders

		public event Action<string> NewInvaderLogEntry;
		void Log(string msg ) {
			NewInvaderLogEntry?.Invoke(msg);
		}

		// =================================================
		// ==============  Invader Actions  ================

		public InvaderDeck InvaderDeck { get; set; }
		public void SkipAllInvaderActions( params Space[] targets ) {
			foreach(var target in targets) {
				ModifyRavage(target, cfg=>cfg.ShouldRavage=false );
				Skip1Build( target );
				SkipExplore(target);
			}
		}

		public void SkipExplore( params Space[] target ) {
			PreExplore.Add( ( gs, args ) => {
				foreach(var space in target)
					args.Skip(space);
				return Task.CompletedTask;
			} );
		}

		public ConfigureRavage GetRavageConfiguration( Space space ) => _ravageConfig.ContainsKey( space ) ? _ravageConfig[space] : new ConfigureRavage();

		public void ModifyRavage( Space space, Action<ConfigureRavage> action ) {
			if(!_ravageConfig.ContainsKey( space ))
				_ravageConfig.Add( space, new ConfigureRavage() );
			action( _ravageConfig[space] );
		}


		void InitRavageFromDeck() {
			this.ScheduledRavageSpaces = InvaderDeck.Ravage
				.SelectMany(card => Island.AllSpaces.Where( card.Matches ))
				.ToList();
		}

		public async Task Ravage( InvaderCard invaderCard ) {
			if(invaderCard == null) return;

			ScheduledRavageSpaces = Island.Boards.SelectMany( board => board.Spaces )
				.Where( invaderCard.Matches )
				.ToList();

			await Ravage();
		}

		public async Task DoInvaderPhase() {

			// Duplicate of InvaderPhase.ActAsync without the logging

			// Blight
			if(BlightCard.IslandIsBlighted) {
				Log( "Island is blighted" );
				await BlightCard.OnStartOfInvaders( this );
			}

			// Fear
			await Fear.Apply();

			// Ravage
			var deck = InvaderDeck;
			Log( "Ravaging:" + deck.Ravage.Select(x=>x.Text).Join("/") );
			await Ravage();

			// Building
			Log( "Building:" + deck.Build.Select(x=>x.Text).Join("/") );
			await Build();

			// Exploring
			deck.TurnOverExploreCards();
			Log( "Exploring:" + (deck.Explore.Count > 0 ? deck.Explore[0].Text : "-") );
			await Explore( deck.Explore.ToArray() );

			deck.Advance();
		}

		/// <summary>
		/// Ravages whatever is in scheduledRavageSpaces
		/// </summary>
		/// <returns></returns>
		public async Task Ravage() {
			await PreRavaging?.InvokeAsync( this, ScheduledRavageSpaces );

			if(ScheduledRavageSpaces==null) throw new InvalidOperationException("dude! you forgot to schedule the ravages.");
			var ravageGroups = ScheduledRavageSpaces
				.Where( x => GetRavageConfiguration( x ).ShouldRavage )
				.Select( x => Invaders.On( x, Cause.Ravage ) )
				.Where( group => group.Tokens.HasInvaders() )
				.Cast<InvaderGroup>()
				.ToArray();

			foreach(var grp in ravageGroups) {
				string ravageSpaceResults = await RavageSpace( grp );
				Log( ravageSpaceResults );
			}
		}

		public virtual async Task<string> RavageSpace( InvaderGroup grp ) {
			var cfg = GetRavageConfiguration( grp.Space );
			var eng = new RavageEngine( this, grp, cfg );
			await eng.Exec();
			return grp.Space.Label + ": " + eng.log.Join( "  " );
		}

		public void SkipRavage( params Space[] spaces ) {
			foreach(var space in spaces)
				ModifyRavage( space, cfg => cfg.ShouldRavage = false );
		}

		readonly Dictionary<Space, ConfigureRavage> _ravageConfig = new Dictionary<Space, ConfigureRavage>(); // change ravage state of a Space

		public void InitBuildFromDeck() {
			ScheduledBuildSpaces = InvaderDeck.Build
				.SelectMany(card => Island.AllSpaces.Where( card.Matches ))
				.GroupBy( s => s )
				.ToDictionary( grp => grp.Key, grp => grp.Count() )
				.ToCountDict();
		}

		public async Task Build( InvaderCard invaderCard ) {

			// Build normal
			ScheduledBuildSpaces = Island.Boards.SelectMany( board => board.Spaces )
				.Where( invaderCard.Matches )
				.GroupBy( s => s )
				.ToDictionary( grp => grp.Key, grp => grp.Count() )
				.ToCountDict();

			await Build();
		}

		public async Task Build() {
			var args = new BuildingEventArgs {
				BuildTypes = new Dictionary<Space, BuildingEventArgs.BuildType>(),
				SpaceCounts = ScheduledBuildSpaces,
			};

			await PreBuilding.InvokeAsync( this, args );

			List<Space> buildSpacesWithInvaders = new List<Space>();
			foreach(var space in args.SpaceCounts.Keys.OrderBy(x=>x.Label) ) {
				int count = args.SpaceCounts[space];
				var tokens = Tokens[space];
				while(count-- > 0) {
					if(tokens.HasInvaders()) {
						var buildType = args.BuildTypes.ContainsKey( space ) 
							? args.BuildTypes[space] 
							: BuildingEventArgs.BuildType.TownsAndCities;
						var buildResult = await Build( Tokens[space], buildType );
						Log( space.Label + ": gets " + buildResult );
					} else {
						Log( space.Label + ": no invaders " );
					}

				}
			}

		}

		public void Skip1Build( params Space[] target ) {
			PreBuilding.Add( (GameState gs, BuildingEventArgs args) => {
				foreach(var skip in target)
					args.SpaceCounts[skip]--;
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

		public async Task Explore( params InvaderCard[] invaderCards ) {

			bool HasTownOrCity( Space space ) { return Tokens[ space ].HasAny(Invader.Town,Invader.City); }

			HashSet<Space> sources = Island.AllSpaces
				.Where( s => s.Terrain == Terrain.Ocean || HasTownOrCity(s) )
				.ToHashSet();

			List<Space> spacesThatMatchCards = Island.Boards.SelectMany( board => board.Spaces )
				.Where( space => invaderCards.Any(card=>card.Matches(space)) )
				.ToList();

			// Run special event cards over it
			var args = new ExploreEventArgs( sources, spacesThatMatchCards );
			await this.PreExplore.InvokeAsync( this, args );

			// not really necessary if we are exposing GameSTate.explorationSpaces

			// Add new spaces
			var spacesToExplore = args.SpacesMatchingCards.Except(args.Skipped)
				.OrderBy(x=>x.Label)
				.Where( space => space.Range( 1 ).Any( sources.Contains ) )
				.ToArray();

			// Explore
			foreach(var b in spacesToExplore)
				await ExploresSpace( b );

		}

		protected virtual async Task ExploresSpace(Space space ) {
			Log(space+":gains explorer");
			await Tokens.Add( Invader.Explorer, space );
		}

		// End of invaders


		#endregion

		#region Dahan Helpers

		public bool DahanIsOn( Space space ) => Tokens[space].Has( TokenType.Dahan );
		public int DahanGetCount( Space space ) =>Tokens[space].Sum( TokenType.Dahan );
		public void DahanAdjust( Space space, int delta = 1 ) => Tokens[space][TokenType.Dahan.Default] += delta;
		public Task DahanDestroy( Space space, int countToDestroy, Token dahanToken, Cause source ) {
			countToDestroy = Math.Min( countToDestroy, DahanGetCount( space ) );
			Tokens[space][dahanToken] -= countToDestroy;
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
			await Invaders.On( space, Cause.Fear ).SmartDamageToGroup( damage );
		}

		#endregion

		public CountDictionary<Space> ScheduledBuildSpaces; // Counts of build spaces
		public List<Space> ScheduledRavageSpaces { get; private set; } // loaded at the beginning of the round with spaces that should get a ravage that round
	}

	public class BuildingEventArgs {
		public CountDictionary<Space> SpaceCounts;
		public Dictionary<Space,BuildType> BuildTypes;
		public enum BuildType { TownsAndCities, TownsOnly, CitiesOnly }
	}

	public class ExploreEventArgs {

		public ExploreEventArgs(HashSet<Space> sources,List<Space> spacesMatchingCards ) {
			this.Sources = sources;
			this.SpacesMatchingCards = spacesMatchingCards.ToImmutableList();
		}

		/// <summary> Towns, cities, and coasts. </summary>
		public HashSet<Space> Sources;

		/// <summary> Should be 2,3 or 4 per board.  (doesn't check sources) </summary>
		public ImmutableList<Space> SpacesMatchingCards;

		public IEnumerable<Space> Skipped => _skipped;

		public void Skip( Space space ) {
			_skipped.Add( space );
		}
		public void SkipAll() {
			_skipped.AddRange(SpacesMatchingCards);
		}

		readonly List<Space> _skipped = new List<Space>();

	}

}
