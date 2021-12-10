using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland {

	public class GameState {

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
			RoundNumber = 1;
			Fear = new Fear( this );
			Invaders = new Invaders( this );
			Tokens = new Tokens_ForIsland( this );

			TimePasses_WholeGame += Heal;
			TimePasses_WholeGame += PreRavaging.ForRound.Clear;
			TimePasses_WholeGame += PreBuilding.ForRound.Clear;
			TimePasses_WholeGame += PreExplore.ForRound.Clear;
		}


		public virtual void Initialize() {

			foreach(var board in Island.Boards)
				foreach(var space in board.Spaces)
					space.InitTokens( Tokens[space] );

			// Explore
			InvaderEngine.Explore( InvaderDeck.Explore[0] ).Wait();

			InvaderDeck.Advance();

			InitSpirits();

			BlightCard.OnGameStart( this );
		}

		void InitSpirits() {
			if(Spirits.Length != Island.Boards.Length)
				throw new InvalidOperationException( "# of spirits and islands must match" );
			for(int i = 0; i < Spirits.Length; ++i)
				Spirits[i].Initialize( Island.Boards[i], this );
		}

		#endregion

		// base-1,  game starts in round-1
		public int RoundNumber { get; private set; }
		public Phase Phase { get; set; }

		// == Components ==
		public Fear Fear { get; }
		public Island Island { get; set; }
		public Spirit[] Spirits { get; }
		public Tokens_ForIsland Tokens { get; }
		public PowerCardDeck MajorCards {get; set; }
		public PowerCardDeck MinorCards { get; set; }
		public InvaderDeck InvaderDeck { 
			get { return _invaderDeck ??= new InvaderDeck(new Random()); }
			set { _invaderDeck = value; }
		}
		InvaderDeck _invaderDeck;
		public Invaders Invaders { get; } // creates ravage/damage objects - Obsolete - just make Tokens do this.
		public int blightOnCard; // 2 per player + 1
		public IBlightCard BlightCard = new NullBlightCard();
		public GameOver Result = null;

		#region Blight

		public async Task DamageLand( Space space, int damageInflictedFromInvaders ) {
			if(damageInflictedFromInvaders==0) return;

			await LandDamaged.InvokeAsync(this,new LandDamagedArgs { Space = space, Damage = damageInflictedFromInvaders} );

			if( damageInflictedFromInvaders > 1)
				await BlightLandWithCascade( space );
		}

		/// <summary>Causes cascading</summary>
		public async Task BlightLandWithCascade( Space blightSpace ){

			var terrainMapper = Island.TerrainMapFor(Cause.Blight);

			while(blightSpace != null) {
				var effect = DetermineAddBlightEffect( this, blightSpace );

				// Each spirit with presence on blight space destorys preseence
				if(effect.DestroyPresence)
					foreach(var spirit in Spirits)
						if(spirit.Presence.IsOn( blightSpace ))
							await spirit.Presence.Destroy( blightSpace, this, Cause.Blight ); // ??? is this correct?  should it be whatever is causing the blight???

				await AddBlight( blightSpace );

				if(BlightCard != null && blightOnCard <= 0)
					BlightCard.OnBlightDepleated( this );

				blightSpace = effect.Cascade
					? await Spirits[0].Action.Decision( new Decision.AdjacentSpace(
						$"Cascade blight from {blightSpace.Label} to",
						blightSpace,
						Decision.AdjacentDirection.Outgoing,
						blightSpace.Adjacent.Where( x => terrainMapper.GetTerrain( x ) != Terrain.Ocean ),
						Present.Always
					) )
					: null;
			}

		}

		/// <summary> Adds blight from the blight card to a space on the board. </summary>
		public async Task AddBlight( Space space, int delta=1 ){ // also used for removing blight
			blightOnCard -= await AddBlightBehavior( Tokens[space], delta );
		}

		public bool HasBlight( Space s ) => GetBlightOnSpace(s) > 0;

		public int GetBlightOnSpace( Space space ){ return Tokens[space].Blight; }

		#endregion

		#region Invader Phase / Deck Modifications

		public void SkipAllInvaderActions( params Space[] targets ) {
			SkipRavage( targets );
			Skip1Build( targets );
			SkipExplore( targets );
		}

		public void SkipRavage( params Space[] spacesToSkip ) {
			PreRavaging.ForRound.Add( ( gs, args ) => {
				foreach(var skip in spacesToSkip)
					args.Skip1(skip);
			} );
		}

		public void AddRavage( Space spacesToAdd ) {
			throw new System.NotImplementedException("!!! should only add to cards that match space");
		}


		public void Skip1Build( params Space[] target ) {
			PreBuilding.ForRound.Add( (GameState gs, BuildingEventArgs args) => {
				foreach(var skip in target)
					args.Skip1(skip);
			});
		}

		public void Add1Build( params Space[] target ) {
			// !!! This should only add to spaces that match invader card
			PreBuilding.ForRound.Add( ( GameState gs, BuildingEventArgs args ) => {
				foreach(var skip in target)
					args.Add( skip );
			} );
		}


		public void SkipExplore( params Space[] target ) {
			PreExplore.ForRound.Add( ( gs, args ) => {
				foreach(var space in target)
					args.Skip(space);
			} );
		}

		public void AddExplore( params Space[] target ) {
			// !!! This should only add to spaces that match invader card
			PreExplore.ForRound.Add( ( gs, args ) => {
				foreach(var space in target)
					args.Add( space );
			} );
		}

		public InvaderEngine InvaderEngine => _invaderEngine ??= BuildInvaderEngine();
		protected virtual InvaderEngine BuildInvaderEngine() => new InvaderEngine(this);
		InvaderEngine _invaderEngine;

		#endregion

		#region Configure Ravage

		public ConfigureRavage GetRavageConfiguration( Space space ) => _ravageConfig.ContainsKey( space ) ? _ravageConfig[space] : new ConfigureRavage();

		public void ModifyRavage( Space space, Action<ConfigureRavage> action ) {
			if(!_ravageConfig.ContainsKey( space ))
				_ravageConfig.Add( space, new ConfigureRavage() );
			action( _ravageConfig[space] );
		}

		readonly Dictionary<Space, ConfigureRavage> _ravageConfig = new Dictionary<Space, ConfigureRavage>(); // change ravage state of a Space

		#endregion

		public DahanGroupBinding DahanOn( Space space ) => Tokens[space].Dahan; // Obsolete - use TargetSpaceCtx

		#region API - overridable

		public Func<GameState,Space,AddBlightEffect> DetermineAddBlightEffect = DefaultDetermineAddBlightEffect; /// <summary> Hook so Stone's presence can stop the cascade / Destroy effects of blight. </summary>

		public Func<TokenCountDictionary,int,Task<int>> AddBlightBehavior = DefaultAddBlight; // hook for Stone's Defiance

		public Func<Spirit,GameState,Cause,Task> Destroy1PresenceFromBlightCard = DefaultDestroy1PresenceFromBlightCard; // Direct distruction from Blight Card, not cascading

		void Heal( GameState obj ) => Healer.Heal( obj ); // called at end of round.
		public Healer Healer = new Healer(); // replacable Behavior

		#endregion

		#region Win / Loss

		// !!! Replace this with an End-Of-Action Event
		public void CheckWinLoss() {
			if( ShouldCheckWinLoss)
				CheckTerrorLevelVictory();
		}
		public bool ShouldCheckWinLoss { private get; set; } 

		// Win Loss Predicates
		static (Func<Space,bool>,string) InvaderCriteria(GameState gs) {
			bool NoCity(Space space) => gs.Tokens[space].Sum(Invader.City)==0;
			bool NoCityOrTown(Space space) => gs.Tokens[space].SumAny(Invader.City,Invader.Town)==0;
			bool NoInvader(Space space) => !gs.Tokens[space].HasInvaders();
			return gs.Fear.TerrorLevel switch {
				3 => (NoCity,"no cities"),
				2 => (NoCityOrTown,"no towns or cities"),
				_ => (NoInvader,"no invaders")
			};
		}

		// ! This is the only thing that needs checked after every action.
		void CheckTerrorLevelVictory(){
			var (filter,description) = InvaderCriteria(this);
			if( Island.AllSpaces.All( filter ) )
				GameOverException.Win($"Terror Level {Fear.TerrorLevel} - {description}");
		}

		#endregion

		#region Default API methods

		static private AddBlightEffect DefaultDetermineAddBlightEffect( GameState gs, Space blightSpace ) {
			bool isFirstBlight = !gs.Tokens[blightSpace].Blight.Any;
			return new AddBlightEffect {
				DestroyPresence = true,
				Cascade = !isFirstBlight,
			};
		}

		/// <returns># of blight to remove from card</returns>
		static Task<int> DefaultAddBlight( TokenCountDictionary tokens, int delta = 1 ) {
			var blight = tokens.Blight;
			if(blight + delta < 0) throw new Exception( "Can't remove blight that isn't there" );

			blight.Add(delta);
			return Task.FromResult(delta);
		}

		static async Task DefaultDestroy1PresenceFromBlightCard( Spirit spirit, GameState gs, Cause cause ) {
			var presence = await spirit.Action.Decision( new Decision.Presence.DeployedToDestory( "Blighted Island: Select presence to destroy.", spirit ) );
			await spirit.Presence.Destroy( presence, gs, cause );
		}

		#endregion

		public void Log( ILogEntry entry ) => NewLogEntry?.Invoke( entry );

		public async Task TriggerTimePasses() {

			_ravageConfig.Clear();

			// Clear Defend
			foreach(var s in Island.AllSpaces)
				Tokens[s][TokenType.Defend] = 0;

			TimePasses_WholeGame?.Invoke( this );

			while(TimePasses_ThisRound.Count > 0)
				await TimePasses_ThisRound.Pop()( this );

			++RoundNumber;
		}

		#region Events

		// - Events -
		public event Action<ILogEntry> NewLogEntry;
		public DualAsyncEvent<RavagingEventArgs> PreRavaging   = new DualAsyncEvent<RavagingEventArgs>();	// A Spread of Rampant Green - Whole game - stop ravage
		public DualAsyncEvent<BuildingEventArgs> PreBuilding   = new DualAsyncEvent<BuildingEventArgs>();	// A Spread of Rampant Green - While game - stop build
		public DualAsyncEvent<ExploreEventArgs>  PreExplore    = new DualAsyncEvent<ExploreEventArgs>();
		public DualAsyncEvent<InvadersRavaged> InvadersRavaged = new DualAsyncEvent<InvadersRavaged>();
		public DualAsyncEvent<LandDamagedArgs> LandDamaged = new DualAsyncEvent<LandDamagedArgs>();

		public event Action<GameState> TimePasses_WholeGame;                                            // Spirit cleanup
		public Stack<Func<GameState, Task>> TimePasses_ThisRound = new Stack<Func<GameState, Task>>(); // This must be Push / Pop

		#endregion

		#region Memento

		public virtual IMemento<GameState> SaveToMemento() => new Memento(this);
		public virtual void LoadFrom( IMemento<GameState> memento ) => ((Memento)memento).Restore(this);

		protected class Memento : IMemento<GameState> {
			public Memento(GameState src) {
				roundNumber  = src.RoundNumber;
				blightOnCard = src.blightOnCard;
				isBlighted   = src.BlightCard.IslandIsBlighted;
				spirits      = src.Spirits.Select(s=>s.SaveToMemento()).ToArray();
				if(src.MajorCards != null) major = src.MajorCards.SaveToMemento();
				if(src.MinorCards != null) minor = src.MinorCards.SaveToMemento();
				invaderDeck  = src.InvaderDeck.SaveToMemento();
				fear         = src.Fear.SaveToMemento();
				tokens       = src.Tokens.SaveToMemento();
			}
			public void Restore(GameState src ) {
				src.RoundNumber = roundNumber;
				src.blightOnCard = blightOnCard;
				src.BlightCard.IslandIsBlighted = isBlighted;
				for(int i=0;i<spirits.Length;++i) src.Spirits[i].LoadFrom( spirits[i] );
				if(src.MajorCards != null ) src.MajorCards.RestoreFrom( major );
				if(src.MinorCards != null ) src.MinorCards.RestoreFrom( minor );
				src.InvaderDeck.LoadFrom( invaderDeck );
				src.Fear.LoadFrom( fear );
				src.Tokens.LoadFrom( tokens );
			}
			readonly int roundNumber;
			readonly int blightOnCard;
			readonly bool isBlighted;
			readonly IMemento<Spirit>[] spirits;
			readonly IMemento<PowerCardDeck> major;
			readonly IMemento<PowerCardDeck> minor;
			readonly IMemento<InvaderDeck> invaderDeck;
			readonly IMemento<Fear> fear;
			readonly IMemento<Tokens_ForIsland> tokens;
		}

		#endregion Memento

	}

	public class Healer {

		public virtual void Heal( GameState gs ) {
			foreach(var space in gs.Tokens.Keys)
				InvaderGroup.HealTokens( gs.Tokens[space] );
		}

	}

	public class LandDamagedArgs {
		public Space Space;
		public int Damage;
	}

	public class AddBlightEffect {
		public bool Cascade;
		public bool DestroyPresence;
	}


}
