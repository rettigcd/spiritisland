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

			TimePasses_WholeGame += PreRavaging.OnEndOfRound;
			TimePasses_WholeGame += PreBuilding.OnEndOfRound;
			TimePasses_WholeGame += PreExplore.OnEndOfRound;
		}

		public virtual void Initialize() {

			foreach(var board in Island.Boards)
				foreach(var space in board.Spaces)
					space.InitTokens( Tokens[space] );

			// Explore
			Task.WaitAll( InvaderEngine.Explore( InvaderDeck.Explore[0] ) );

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
			PreRavaging.ForThisRound( ( gs, args ) => {
				foreach(var skip in spacesToSkip)
					args.Skip1(skip);
			} );
		}

		public void Skip1Build( params Space[] target ) {
			PreBuilding.ForThisRound( (GameState gs, BuildingEventArgs args) => {
				foreach(var skip in target)
					args.Skip1(skip);
			});
		}

		public void SkipExplore( params Space[] target ) {
			PreExplore.ForThisRound( ( gs, args ) => {
				foreach(var space in target)
					args.Skip(space);
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
			int newCount = blight + delta;
			if(newCount < 0) throw new Exception( "Can't remove blight that isn't there" );

			blight.Count = newCount;
			return Task.FromResult(delta);
		}

		static async Task DefaultDestroy1PresenceFromBlightCard( Spirit spirit, GameState gs, Cause cause ) {
			var presence = await spirit.Action.Decision( new Decision.Presence.DeployedToDestory( "Blighted Island: Select presence to destroy.", spirit ) );
			await spirit.Presence.Destroy( presence, gs, cause );
		}

		#endregion

		#region API

		public void Log( ILogEntry entry ) => NewLogEntry?.Invoke( entry );

		public async Task TriggerTimePasses() {

			// stack allows us to unwind items in reverse order from when we set them up
			while(TimePasses_ThisRound.Count > 0) 
				await TimePasses_ThisRound.Pop()( this );

			_ravageConfig.Clear();

			// Clear Defend
			foreach(var s in Island.AllSpaces)
				Tokens[s][TokenType.Defend] = 0;

			TimePasses_WholeGame?.Invoke( this );
			++RoundNumber;
		}

		#endregion

		#region Events

		// - Events -
		public event Action<ILogEntry> NewLogEntry;
		public AsyncEvent<RavagingEventArgs> PreRavaging   = new AsyncEvent<RavagingEventArgs>();	// A Spread of Rampant Green - stop ravage
		public AsyncEvent<BuildingEventArgs> PreBuilding   = new AsyncEvent<BuildingEventArgs>();	// A Spread of Rampant Green - stop build
		public AsyncEvent<ExploreEventArgs> PreExplore     = new AsyncEvent<ExploreEventArgs>();
		public AsyncEvent<InvadersRavaged> InvadersRavaged = new AsyncEvent<InvadersRavaged>();
		public event Action<GameState> TimePasses_WholeGame;												// Spirit cleanup
		public Stack<Func<GameState, Task>> TimePasses_ThisRound = new Stack<Func<GameState, Task>>();      // Gift of Power
		public AsyncEvent<LandDamagedArgs> LandDamaged = new AsyncEvent<LandDamagedArgs>();

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

	public class LandDamagedArgs {
		public Space Space;
		public int Damage;
	}

	public class AddBlightEffect {
		public bool Cascade;
		public bool DestroyPresence;
	}


}
