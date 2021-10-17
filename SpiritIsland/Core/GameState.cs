using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland {


	public class GameState {

		// base-1,  game starts in round-1
		public int RoundNumber { get; private set; }

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

			TimePassed += PreRavaging.OnEndOfRound;
			TimePassed += PreBuilding.OnEndOfRound;
			TimePassed += PreExplore.OnEndOfRound;
		}

		public virtual void Initialize() {

			// Custom Deck not supplied, use default deck
			if(InvaderDeck == null)
				InvaderDeck = new InvaderDeck(new Random());

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

		// == Components ==
		public Fear Fear { get; }
		public Island Island { get; set; }
		public Spirit[] Spirits { get; }
		public Tokens_ForIsland Tokens { get; }
		public PowerCardDeck MajorCards {get; set; }
		public PowerCardDeck MinorCards { get; set; }
		public InvaderDeck InvaderDeck { get; set; }

		#region Time Passes

		public event Action<GameState> TimePassed;												// Spirit cleanup
		public Stack<Func<GameState, Task>> TimePasses_ThisRound = new Stack<Func<GameState, Task>>();        // Gift of Power

		public async Task TimePasses() {

			await ExecuteAndClear_OneRoundEvents(); // this is async because of Gift of Contancy has user action 'at end of turn'

			// Clear Defend
			foreach(var s in Island.AllSpaces)
				Tokens[s][TokenType.Defend] = 0;

			TimePassed?.Invoke( this );
			++RoundNumber;
		}

		async Task ExecuteAndClear_OneRoundEvents() {
			_ravageConfig.Clear();

			// stack allows us to unwind items in reverse order from when we set them up
			while(TimePasses_ThisRound.Count > 0) 
				await TimePasses_ThisRound.Pop()( this );
		}

		#endregion

		#region Blight

		public int blightOnCard; // 2 per player
		public IBlightCard BlightCard = new NullBlightCard();

		/// <summary>Causes cascading</summary>
		public async Task BlightLand( Space blightSpace ){

			var terrainMapper = Island.TerrainMapFor(Cause.Blight);

			while(blightSpace != null) {

				var tokens = Tokens[blightSpace];
				bool isFirstBlight = tokens.Blight.Count == 0;

				foreach(var spirit in Spirits)
					if(spirit.Presence.IsOn( blightSpace ))
						spirit.Presence.Destroy( blightSpace );

				tokens.Blight.Count++;
				--blightOnCard;

				if(BlightCard != null && blightOnCard <= 0)
					BlightCard.OnBlightDepleated( this );

				blightSpace = isFirstBlight ? null
					: await Spirits[0].Action.Decision(new Decision.AdjacentSpace(
						$"Cascade blight from {blightSpace.Label} to", 
						blightSpace, 
						Decision.AdjacentDirection.Outgoing,
						blightSpace.Adjacent.Where( x => terrainMapper.GetTerrain( x ) != Terrain.Ocean ),
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

		public AsyncEvent<RavagingEventArgs> PreRavaging = new AsyncEvent<RavagingEventArgs>();				// A Spread of Rampant Green - stop ravage
		public AsyncEvent<BuildingEventArgs> PreBuilding = new AsyncEvent<BuildingEventArgs>();	// A Spread of Rampant Green - stop build
		public AsyncEvent<ExploreEventArgs> PreExplore = new AsyncEvent<ExploreEventArgs>();

		public InvaderEngine InvaderEngine => _invaderEngine ??= BuildInvaderEngine();
		protected virtual InvaderEngine BuildInvaderEngine() => new InvaderEngine(this);
		InvaderEngine _invaderEngine;

		public event Action<string> NewInvaderLogEntry;
		public void Log(string msg ) =>	NewInvaderLogEntry?.Invoke(msg);

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

		#region Invader Helpers - cleanup!

		public Invaders Invaders { get; } // creates ravage/damage objects - Obsolete - just make Tokens do this.

		#endregion

		#region Dahan Helpers

		public bool DahanIsOn( Space space ) => Tokens[space].Has( TokenType.Dahan );
		public int DahanGetCount( Space space ) =>Tokens[space].Sum( TokenType.Dahan );
		public void DahanAdjust( Space space, int delta = 1 ) => Tokens[space][TokenType.Dahan.Default] += delta;

		/// <remarks>Called from (1) Ravage Engine and (2) Spirit-de</remarks>
		public Task DestroyToken( Space space, int countToDestroy, Token token, Cause source ) {
			var tokens = Tokens[space];
			countToDestroy = Math.Min( countToDestroy, tokens[token] );
			tokens[token] -= countToDestroy;
			return Tokens.TokenDestroyed.InvokeAsync( this, new TokenDestroyedArgs {
				Token = token.Generic,
				space = space,
				count = countToDestroy,
				Source = source
			} );
		}

		#endregion

		public Task Move( Token invader, Space from, Space to ) => Tokens.Move( invader, from, to );

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
				if(src.MajorCards != null ) src.MajorCards.LoadFrom( major );
				if(src.MinorCards != null ) src.MinorCards.LoadFrom( minor );
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

}
