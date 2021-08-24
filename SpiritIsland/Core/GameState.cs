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
				this.AddFearCard( new NullFearCard() );
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

		public void Initialize() {

			foreach(var board in Island.Boards)
				InitItemsMarkedOnBoard( board );

			Explore( InvaderDeck.Explore );
			InvaderDeck.Advance();
			InitSpirits();

			while(FearDeck.Count<9)
				AddFearCard(new NullFearCard());

			BlightCard.OnGameStart(this);
		}

		// == EVENTS ==
		public AsyncEvent<Space[]> PreRavaging = new AsyncEvent<Space[]>();						// A Spread of Rampant Green - stop ravage
		public AsyncEvent<Space[]> PreBuilding = new AsyncEvent<Space[]>();						// A Spread of Rampant Green - stop build
		public AsyncEvent<DahanMovedArgs> DahanMoved = new AsyncEvent<DahanMovedArgs>();                    // Thunderspeaker
		public AsyncEvent<InvaderMovedArgs> InvaderMoved = new AsyncEvent<InvaderMovedArgs>();                    // Thunderspeaker
		public AsyncEvent<DahanDestroyedArgs> DahanDestroyed = new AsyncEvent<DahanDestroyedArgs>();		// Thunderspeaker
		public event Action<GameState> TimePassed;												// Spirit cleanup
		// == Single Round hooks
		public SyncEvent<FearArgs> FearAdded_ThisRound = new SyncEvent<FearArgs>();						// Dread Apparations
		public Stack<Func<GameState, Task>> TimePasses_ThisRound = new Stack<Func<GameState, Task>>();  // Gift of Power
		readonly Dictionary<Space, ConfigureRavage> _ravageConfig = new Dictionary<Space, ConfigureRavage>(); // change ravage state of a Space

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
			await ExecuteAndClear_OneRoundEvents();
			TimePassed?.Invoke( this );
			++Round;
		}

		async Task ExecuteAndClear_OneRoundEvents() {
			_ravageConfig.Clear();

			// stack allows us to unwind items in reverse order from when we set them up
			while(TimePasses_ThisRound.Count > 0)
				await TimePasses_ThisRound.Pop()( this );

			// clean out the 1-round-only events
			FearAdded_ThisRound.Handlers.Clear();
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
			blightCount[space] += counts.Blight; // don't use AddBlight because that pulls it from the card and triggers blighted island
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

		#region Wilds
		public void AddWilds( Space space ){ wildsCount[space]++; }
		public bool HasWilds( Space s ) => wildsCount[s] > 0;
		#endregion

		#region Fear

		public void AddFearCard(IFearCard fearCard ) {
			if(FearDeck.Count >= 9) throw new InvalidOperationException("Fear deck is full.");
			var labels = new string[]{"1-A","1-B","1-C","2-A","2-B","2-C","3-A","3-B","3-C" };
			int index = 9- FearDeck.Count-1;
			var td = new NamedFearCard { Card = fearCard, Text = "Lvl "+labels[index]};
			FearDeck.Push( td ); 
		}

		public readonly Stack<NamedFearCard> FearDeck = new Stack<NamedFearCard>();
		public readonly Stack<NamedFearCard> ActivatedFearCards = new Stack<NamedFearCard>();
		public int TerrorLevel { get{
			int ct = FearDeck.Count;
			int terrorLevel = ct > 6 ? 1 : ct > 3 ? 2 : 1;
			return terrorLevel;
		} }


		public void AddFearDirect( FearArgs args ) {
			FearPool += args.count;
			if(4 <= FearPool) { // should be while() - need unit test
				FearPool -= 4;
				ActivatedFearCards.Push( FearDeck.Pop() );
				ActivatedFearCards.Peek().Text = "Active " + ActivatedFearCards.Count;
			}
			if(FearDeck.Count == 0)
				GameOverException.Win();
			FearAdded_ThisRound?.Invoke(this,args);
		}

		#endregion

		#region Dahan
		public void AdjustDahan( Space space, int delta=1 ){ 	
			dahanCount[space]+=delta;
		}

		public Task DestroyDahan(Space space,int countToDestroy, Cause source ) {
			countToDestroy = Math.Min(countToDestroy,DahanCount(space));
			AdjustDahan(space, -countToDestroy );
			return DahanDestroyed.InvokeAsync(this,new DahanDestroyedArgs { space = space, count=countToDestroy, Source = source } );
		}

		public Task MoveDahan( Space from, Space to, int count = 1 ) {
			AdjustDahan(from,-count);
			AdjustDahan(to,count);
			return DahanMoved.InvokeAsync(this, new DahanMovedArgs { from = from, to = to, count = count } );
		}

		public int DahanCount( Space space ){ return dahanCount[space]; }
		public bool HasDahan( Space space ) => DahanCount(space)>0;
		#endregion

		#region Invaders

		public async Task ApplyFear() {
			while( ActivatedFearCards.Count > 0) {
				NamedFearCard fearCard = ActivatedFearCards.Pop();
				// show card to each user
				foreach(var spirit in Spirits)
					await spirit.ShowFearCardToUser( "Activating Fear", fearCard );
				await fearCard.Card.Level1(this);
			}
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

		public ConfigureRavage GetRavageConfiguration(Space space) => _ravageConfig.ContainsKey(space) ? _ravageConfig[space] : new ConfigureRavage();
		public void ModRavage(Space space,Action<ConfigureRavage> action) {
			if(!_ravageConfig.ContainsKey(space)) 
				_ravageConfig.Add(space,new ConfigureRavage());
			action(_ravageConfig[space]);
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
			return new InvaderGroup( targetSpace, this.GetCounts( targetSpace ), AddFearDirect, Cause.Ravage );
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
		readonly CountDictionary<Space> beastCount = new CountDictionary<Space>();
		readonly CountDictionary<Space> wildsCount = new CountDictionary<Space>();

		readonly Dictionary<Space,int[]> invaderCount = new Dictionary<Space,int[]>();


		public int[] GetCounts(Space space) {
			if(invaderCount.ContainsKey(space)) return invaderCount[space];
			return invaderCount[space] = new int[InvaderSpecific.TypesCount+1]; // 1 for the total
		}

		readonly CountDictionary<Space> dahanCount = new CountDictionary<Space>();

		public int FearPool {get; private set; } = 0;

		public PowerCardDeck MajorCards;
		public PowerCardDeck MinorCards;

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
			catch(Exception ex) {
				int i = 0; // do something here
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


	public class DahanMovedArgs {
		public Space from;
		public Space to;
		public int count;
	};

	public class InvaderMovedArgs {
		public InvaderSpecific Invader;
		public Space from;
		public Space to;
	};


	public enum Cause {
		None,
		Invaders,
		Power,
		Ravage
	}

	public class DahanDestroyedArgs {
		public Space space;
		public int count;
		public Cause Source;
	};

	public class FearArgs {
		public int count;
		public Space space;
		public Cause cause;
	}

	public class NullFearCard : IFearCard {
	
		public const string Name = "Null Fear Card";
		public Task Level1( GameState gs ) { return Task.CompletedTask; }
		public Task Level2( GameState gs ) { return Task.CompletedTask; }
		public Task Level3( GameState gs ) { return Task.CompletedTask; }
	}


}
