using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland {

	public class GameState {

		// base-1,  game starts in round-1
		public int Round { get; private set; }

		public GameState(params Spirit[] spirits){
			if(spirits.Length==0) throw new ArgumentException("Game must include at least 1 spirit");
			this.Spirits = spirits;
			InvaderDeck = new InvaderDeck();
			Round = 1;
			// ! is there a better way to disable fear-win during tests
			while(FearDeck.Count < 9)
				FearDeck.Push( new NullFearCard() );
		}

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

		public InvaderDeck InvaderDeck { get; set; }
		public Island Island { get; set; }
		public Spirit[] Spirits { get; }

		public void InitIsland() {

			foreach(var board in Island.Boards)
				InitItemsMarkedOnBoard( board );

			Explore( InvaderDeck.Explore );
			InvaderDeck.Advance();
			InitSpirits();

			while(FearDeck.Count<9)
				FearDeck.Push(new NullFearCard());

			BlightCard.OnGameStart(this);
		}
		class NullFearCard : IFearCard {
			public Task Level1( GameState gs ) {return Task.CompletedTask;}
			public Task Level2( GameState gs ) { return Task.CompletedTask; }
			public Task Level3( GameState gs ) { return Task.CompletedTask; }
		}


		public event Action<GameState> TimePassed;
		public AsyncEvent<Space[]> PreRavaging = new AsyncEvent<Space[]>();
		public AsyncEvent<Space[]> PreBuilding = new AsyncEvent<Space[]>();
		public AsyncEvent<DahanMovedArgs> DahanMoved = new AsyncEvent<DahanMovedArgs>();
		public AsyncEvent<DahanDestroyedArgs> DahanDestroyed = new AsyncEvent<DahanDestroyedArgs>();
		public Stack<Func<GameState,Task>> EndOfRoundCleanupAction = new Stack<Func<GameState,Task>>();

		void InitSpirits() {
			if(Spirits.Length != Island.Boards.Length)
				throw new InvalidOperationException( "# of spirits and islands must match" );
			for(int i = 0; i < Spirits.Length; ++i)
				Spirits[i].Initialize( Island.Boards[i], this );
		}

		public async Task TimePasses() {
			// heal
			foreach(var pair in invaderCount) 
				new InvaderGroup(pair.Key,pair.Value,null).Heal();

			_ravageConfig.Clear();

			// stack allows us to unwind items in reverse order from when we set them up
			while(EndOfRoundCleanupAction.Count>0)
				await EndOfRoundCleanupAction.Pop()(this);

			TimePassed?.Invoke(this);

			++Round;
		}



		void InitItemsMarkedOnBoard(Board board) {
			foreach(var space in board.Spaces)
				InitSpace( space );
		}

		void InitSpace( Space space ) {
			var counts = space.StartUpCounts;
			Adjust( space, InvaderSpecific.City, counts.Cities );
			Adjust( space, InvaderSpecific.Town, counts.Towns );
			Adjust( space, InvaderSpecific.Explorer, counts.Explorers );
			this.AdjustDahan( space, counts.Dahan );
			if(counts.Blight > 0) this.AddBlight( space ); // add 1
		}

		public void Defend( Space space, int delta ) {
			ModRavage(space, cfg=>cfg.Defend += delta);
		}
		public int GetDefence(Space space) => GetRavageConfiguration(space).Defend;

		#region Beasts
		public void AddBeast( Space space ){ beastCount[space]++; }
		public bool HasBeasts( Space s ) => beastCount[s] > 0;
		#endregion

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
		public void RemoveBlight( Space space){
			if(blightCount[space]==0) return;
			blightCount[space]--;
			blightOnCard++;
		}

		public void AddBlight( Space space, int delta=1 ){ blightCount[space]+=delta; }

		public bool HasBlight( Space s ) => blightCount[s] > 0;
		public int GetBlightOnSpace( Space space ){ return blightCount[space]; }

		public Stack<Space> cascadingBlight = new Stack<Space>();

		#endregion

		#region Wilds
		public void AddWilds( Space space ){ wildsCount[space]++; }
		public bool HasWilds( Space s ) => wildsCount[s] > 0;
		#endregion

		#region Fear

		public readonly Stack<IFearCard> FearDeck = new Stack<IFearCard>();
		public readonly Stack<IFearCard> ActivatedFearCards = new Stack<IFearCard>();
		public int TerrorLevel { get{
			int ct = FearDeck.Count;
			int terrorLevel = ct > 6 ? 1 : ct > 3 ? 2 : 1;
			return terrorLevel;
		} }

		public void AddFear(int count) {
			FearPool += count;
			if(4 <= FearPool) { // should be while() - need unit test
				FearPool -= 4;
				ActivatedFearCards.Push( FearDeck.Pop() );
			}
			if(FearDeck.Count==0)
				GameOverException.Win();

		}
		#endregion

		#region Dahan
		public void AdjustDahan( Space space, int delta=1 ){ 	
			dahanCount[space]+=delta;
		}

		public Task DestoryDahan(Space space,int countToDestroy, DahanDestructionSource source ) {
			countToDestroy = Math.Min(countToDestroy,GetDahanOnSpace(space));
			AdjustDahan(space, -countToDestroy );
			return DahanDestroyed.Invoke(this,new DahanDestroyedArgs { space = space, count=countToDestroy, Source = source } );
		}

		public Task MoveDahan( Space from, Space to, int count = 1 ) {
			AdjustDahan(from,-count);
			AdjustDahan(to,count);
			return DahanMoved.Invoke(this, new DahanMovedArgs { from = from, to = to, count = count } );
		}

		public int GetDahanOnSpace( Space space ){ return dahanCount[space]; }
		public bool HasDahan( Space space ) => GetDahanOnSpace(space)>0;
		#endregion

		#region Invaders

		public async Task ApplyFear() {
			while( ActivatedFearCards.Count > 0 )
				await ActivatedFearCards.Pop().Level1(this);
		}

		public async Task<string[]> Ravage( InvaderCard invaderCard ) {
			if(invaderCard == null) return Array.Empty<string>();

			var initialRavageSpaces = Island.Boards.SelectMany( board => board.Spaces )
				.Where( invaderCard.Matches )
				.Where( x=>GetRavageConfiguration(x).ShouldRavage )
				.ToArray();

			PreRavaging?.Invoke( this, initialRavageSpaces );

			var ravageSpaces = initialRavageSpaces
				.Where( x => GetRavageConfiguration( x ).ShouldRavage ) // not sure this is necessary since it is called during execute
				.ToArray();

			InvaderGroup[] ravageGroups = ravageSpaces
				.Select( InvadersOn )
				.Where( group => group.InvaderTypesPresent_Specific.Any() )
				.ToArray();

			var msgs = new List<string>();
			foreach(var grp in ravageGroups) {
				var eng = new RavageEngine( this, grp, GetRavageConfiguration(grp.Space) );
				await eng.Exec();
				msgs.Add( grp.Space.Label + ": " + eng.log.Join( "  " ) );
			}
			return msgs.ToArray();
		}

		public ConfigureRavage GetRavageConfiguration(Space space) => _ravageConfig.ContainsKey(space) ? _ravageConfig[space] : new ConfigureRavage();
		readonly Dictionary<Space,ConfigureRavage> _ravageConfig = new Dictionary<Space, ConfigureRavage>();
		public void ModRavage(Space space,Action<ConfigureRavage> action) {
			if(!_ravageConfig.ContainsKey(space)) 
				_ravageConfig.Add(space,new ConfigureRavage());
			action(_ravageConfig[space]);
		}


		public string[] Build( InvaderCard invaderCard ) {

			var buildLands = Island.Boards.SelectMany( board => board.Spaces )
				.Where( invaderCard.Matches )
				.Except( skipBuild )
				.ToArray();

			PreBuilding?.Invoke(this,buildLands);

			buildLands = buildLands.Except( skipBuild ).ToArray(); // reload in case they changed

			return buildLands
				.Select( InvadersOn )
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
						 || space.SpacesWithin( 1 ).Any( HasTownOrCity )
				)
				.Except( skipExplore )
				.ToArray();

			foreach(var space in exploredSpaces)
				Adjust( space, InvaderSpecific.Explorer, 1 );
			return exploredSpaces;
		}

		public bool HasInvaders( Space space ) 
			=> GetCounts(space).Any(x=>x>0);

		public void Adjust(Space space, InvaderSpecific invader, int count){
			InvaderGroup.Adjust( GetCounts( space ), invader, count);
		}
		public void Move(InvaderSpecific invader, Space from, Space to ) {
			Adjust( from,invader,-1);
			Adjust( to, invader, 1 );
		}

		public InvaderGroup InvadersOn(Space targetSpace) {
			return InitInvaderGroup(targetSpace);
		}

		InvaderGroup InitInvaderGroup(Space targetSpace) {
			return new InvaderGroup( targetSpace, this.GetCounts(targetSpace), AddFear );
		}

		string Build( InvaderGroup group ) {
			int townCount = group[InvaderSpecific.Town] + group[InvaderSpecific.Town1];
			int cityCount = group[InvaderSpecific.City] + group[InvaderSpecific.City2] + group[InvaderSpecific.City1];
			var invaderToAdd = townCount > cityCount ? InvaderSpecific.City : InvaderSpecific.Town;
			Adjust( group.Space, invaderToAdd, 1 );
			return $"{group.Space.Label} gets {invaderToAdd.Generic.Label}";
		}


		public void DamageInvaders(Space space,int damage){ // !!! let players choose the item to apply damage to
			if(damage==0) return;
			InvadersOn(space).ApplySmartDamageToGroup( damage );
		}

		#endregion

		readonly CountDictionary<Space> blightCount = new CountDictionary<Space>();
		readonly CountDictionary<Space> beastCount = new CountDictionary<Space>();
		readonly CountDictionary<Space> wildsCount = new CountDictionary<Space>();

		readonly Dictionary<Space,int[]> invaderCount = new Dictionary<Space,int[]>();

		int[] GetCounts(Space space ) {
			if(invaderCount.ContainsKey(space)) return invaderCount[space];
			return invaderCount[space] = new int[InvaderSpecific.TypesCount+1]; // 1 for the total
		}

		readonly CountDictionary<Space> dahanCount = new CountDictionary<Space>();

		public int FearPool {get; private set; } = 0;

		public PowerCardDeck MajorCards;
		public PowerCardDeck MinorCards;
	}

	public class PowerCardDeck {

		public PowerCardDeck(IList<PowerCard> cards ) {
			discards = cards.ToList();
		}

		public PowerCard FlipNext() {
			if(cards.Count == 0)
				ReshuffleDiscardDeck();
			var next = cards.Pop();
			return next;
		}

		public List<PowerCard> Flip( int count ) {
			var flipped = new List<PowerCard>();
			for(int i = 0; i < count; ++i) flipped.Add( FlipNext() );
			return flipped;
		}

		public void Discard(IEnumerable<PowerCard> discards) => this.discards.AddRange(discards);

		void ReshuffleDiscardDeck() {
			discards.Shuffle();
			foreach(var card in discards) cards.Push(card);
			discards.Clear();
		}

		readonly Stack<PowerCard> cards = new Stack<PowerCard>();
		readonly List<PowerCard> discards;

	}


	public class AsyncEvent<T> {
		public async Task Invoke(GameState gameState,T t) {
			var handlerCopy = Handlers.ToArray(); // so handlers can remove themselvers from the collection while looping over it (Collection was modified;)
			foreach(var handler in handlerCopy)
				await handler( gameState,t );
		}

		public List<Func<GameState,T,Task>> Handlers = new List<Func<GameState, T,Task>>();
	}

	public class DahanMovedArgs {
		public Space from;
		public Space to;
		public int count;
	};

	public enum DahanDestructionSource { Invaders,
		PowerCard
	}

	public class DahanDestroyedArgs {
		public Space space;
		public int count;
		public DahanDestructionSource Source;
	};




}
