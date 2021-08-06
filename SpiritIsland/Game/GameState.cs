using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland {

	public class GameState {

		// base-1,  game starts in round-1
		public int Round { get; private set; }

		public GameState(params Spirit[] spirits){
			if(spirits.Length==0) throw new ArgumentException("Game must include at least 1 spirit");
			this.Spirits = spirits;
			InvaderDeck = new InvaderDeck();
			Round = 1;
		}

		internal void SkipAllInvaderActions( Space target ) {
			skipRavage.Add(target);
			skipBuild.Add( target );
			skipExplore.Add(target);
		}

		internal void SkipBuild( params Space[] target ) {
			skipBuild.AddRange( target );
		}

		internal void SkipExplore( params Space[] target ) {
			skipExplore.AddRange( target );
		}

		readonly List<Space> skipRavage = new List<Space>();
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

		private void InitSpirits() {
			if(Spirits.Length != Island.Boards.Length)
				throw new InvalidOperationException( "# of spirits and islands must match" );
			for(int i = 0; i < Spirits.Length; ++i)
				Spirits[i].Initialize( Island.Boards[i], this );
		}

		public event Action TimePassed;

		public void TimePasses() {
			// heal
			var damaged = invaderCount.Keys
				.Where(p=>p.Invader != p.Invader.Healthy)
				.ToArray();
			foreach(var pair in damaged){
				// heal those still alive
				if(pair.Invader.Health > 0)
					invaderCount[pair.ToHealthy()] += invaderCount[pair];
				// remove damaged unit
				invaderCount[pair] = 0;
			}
			defendCount.Clear();
			++Round;

			TimePassed?.Invoke();
		}

		void InitItemsMarkedOnBoard(Board board) {
			foreach(var space in board.Spaces)
				InitSpace( space );
		}

		void InitSpace( Space space ) {
			var counts = space.StartUpCounts;
			this.Adjust( space, Invader.City, counts.Cities );
			this.Adjust( space, Invader.Town, counts.Towns );
			this.Adjust( space, Invader.Explorer, counts.Explorers );
			this.AddDahan( space, counts.Dahan );
			if(counts.Blight > 0) this.AddBlight( space ); // add 1
		}

		public void Defend( Space space, int delta ) {
			defendCount[space] += delta;
		}
		public int GetDefence(Space space) => defendCount[space];

		#region Beasts
		public void AddBeast( Space space ){ beastCount[space]++; }
		public bool HasBeasts( Space s ) => beastCount[s] > 0;
		#endregion

		#region Blight

		public int blightOnCard; // 2 per player
		public IBlightCard BlightCard = new NullBlightCard();

		/// <summary>Causes cascading</summary>
		public void BlightLand( Space space ){
			if(HasBlight(space))
				cascadingBlight.Push(space);
			AddBlight(space);

			foreach(var spirit in Spirits)
				if(spirit.Presence.Contains(space))
					spirit.Presence.Remove(space);

			--blightOnCard;
			if(BlightCard != null && blightOnCard==0)
				BlightCard.OnBlightDepleated(this);

		}
		public void RemoveBlight( Space space){
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
		public void AddDahan( Space space, int delta=1 ){ 	dahanCount[space]+=delta;}
		public int GetDahanOnSpace( Space space ){ return dahanCount[space]; }
		public bool HasDahan( Space space ) => GetDahanOnSpace(space)>0;
		#endregion

		#region Invaders

		public async Task ApplyFear() {
			while( ActivatedFearCards.Count > 0 )
				await ActivatedFearCards.Pop().Level1(this);
		}

		public string[] Ravage( InvaderCard invaderCard ) {
			if(invaderCard == null) return Array.Empty<string>();

			// 1 point of damage,  Prefer C@1  ---  ---  T@1  ---  E@1 >>> C@2, T@2, C@3
			// 2 points of damage, Prefer C@1  C@2  ---  T@1  T@2  E@1 >>> C@3
			// 3 points of damage, Prefer C@1  C@2  C@3  T@1  T@2  E@1
			var ravageSpaces = Island.Boards.SelectMany( board => board.Spaces )
				.Where( invaderCard.Matches )
				.Except( skipRavage )
				.ToArray();

			Ravaging?.Invoke( this, ravageSpaces );

			var ravageGroups = ravageSpaces
				.Select( InvadersOn )
				.Where( group => group.InvaderTypesPresent.Any() )
				.ToArray();

			return ravageGroups
				.Select( RavageSpace )
				.ToArray();
		}

		public string[] Build( InvaderCard invaderCard ) {
			return Island.Boards.SelectMany( board => board.Spaces )
				.Where( invaderCard.Matches )
				.Except( skipBuild )
				.Select( InvadersOn )
				.Where( group => group.InvaderTypesPresent.Any() )
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
				Adjust( space, Invader.Explorer, 1 );
			return exploredSpaces;
		}

		public bool HasInvaders( Space space ) 
			=> invaderCount.Keys.Any(k=>k.Space==space);

		public void Adjust(Space space, Invader invader, int count){
			invaderCount[Key(space,invader)] += count;
		}

		public InvaderGroup InvadersOn(Space targetSpace) {
			return InitInvaderGroup(targetSpace);
		}

		public void UpdateFromGroup(InvaderGroup invaders){

			string x = invaders.Space + ": " + invaders.Changed
				.Select(x=>x.Summary+"="+invaders[x])
				.Join(", ");

			AddFear( invaders.DestroyedCities * 2 );
			AddFear( invaders.DestroyedTowns );

			foreach(var invader in invaders.Changed)
				this.invaderCount[Key(invaders.Space,invader)] = invaders[invader];


		}

		InvaderGroup InitInvaderGroup(Space targetSpace) {
			var dict1 = invaderCount.Keys
				.Where(k=>k.Space==targetSpace)
				.ToDictionary(k=>k.Invader,k=>invaderCount[k]);
			return new InvaderGroup( targetSpace, dict1 );
		}

		string Build( InvaderGroup group ) {
            int townCount = group[Invader.Town] + group[Invader.Town1];
            int cityCount = group[Invader.City] + group[Invader.City2] + group[Invader.City1];
            var invaderToAdd = townCount > cityCount ? Invader.City : Invader.Town;
            Adjust( group.Space, invaderToAdd, 1 );
			return $"{group.Space.Label} gets {invaderToAdd.Label}";
		}

		Invader[] KillOrder => killOrder ??= "C@1 C@2 C@3 T@1 T@2 E@1".Split(' ').Select(k=>Invader.Lookup[k]).ToArray();
		Invader[] killOrder;


		Invader[] LeftOverOrder => leftOverOrder ??="C@2 T@2 C@3".Split(' ').Select(k=>Invader.Lookup[k]).ToArray();
		Invader[] leftOverOrder;

		/// <summary> Fired before ravage occurs</summary>
		public event Action<GameState,Space[]> Ravaging;

		string RavageSpace( InvaderGroup ravageGroup ) {

			var log = new List<String>();

			int damageFromInvaders = ravageGroup.DamageInflictedByInvaders;
			int dahan = GetDahanOnSpace( ravageGroup.Space );

			if(damageFromInvaders==0) log.Add("-no ravage-");

			if(damageFromInvaders>0){
				// calculate damage from invaders
				int defend = defendCount[ravageGroup.Space];
				int damageInflictedFromInvaders = Math.Max( damageFromInvaders - defend, 0);
				log.Add($"{ravageGroup} inflicts {damageFromInvaders}-{defend}={damageInflictedFromInvaders} damage.");

				// damage: Land
				bool blight = damageInflictedFromInvaders>1;
				if(blight){
					BlightLand(ravageGroup.Space);
					log.Add("Blights land.");
				}
				// damage: Dahan
				if(noDamageToDahan.Contains(ravageGroup.Space)){ // Conceiling Shadows
					log.Add("Dahan - protected from damange.");
					noDamageToDahan.Remove(ravageGroup.Space);
				} else {
					int dahanKilled = Math.Min( damageInflictedFromInvaders / 2, dahan ); // rounding down
					if(dahanKilled>0){
						AddDahan( ravageGroup.Space, -dahanKilled ); 
						int remainingDahan = dahan - dahanKilled;
						log.Add($"Kills {dahanKilled} of {dahan} Dahan leaving {remainingDahan} Dahan.");
						dahan = remainingDahan;
					}
				}

				// damage: Invaders
				if(dahan>0)
					ApplyDamageToInvaders( ravageGroup, dahan * 2, log );
			}

			defendCount[ravageGroup.Space] = 0;
			return ravageGroup.Space.Label+": "+log.Join("  ");
		}


		public void DamageInvaders(Space space,int damage){ // !!! let players choose the item to apply damage to
			if(damage==0) return;
			var group = InvadersOn(space);
			ApplyDamageToInvaders(group,damage);
		}

		void ApplyDamageToInvaders( InvaderGroup ravageGroup, int startingDamage, List<string> log = null ) {
			int damageToInvaders = startingDamage;

			// While damage remains    &&    we have invaders
			while(damageToInvaders > 0 && ravageGroup.InvaderTypesPresent.Any()) {
				var invaderToDamage = KillOrder
					.FirstOrDefault( invader =>
						invader.Health <= damageToInvaders // prefer things we can kill
						&& ravageGroup[invader] > 0
					)
					?? LeftOverOrder.First( invader => ravageGroup[invader] > 0 ); // left-over damage

				damageToInvaders -= ravageGroup.ApplyDamageMax(invaderToDamage,damageToInvaders);

			}
			if(log!=null) log.Add($"{startingDamage} damage to invaders leaving {ravageGroup}." );
			UpdateFromGroup( ravageGroup );
		}

		static InvaderKey Key(Space space,Invader invader) => new InvaderKey{ Invader=invader, Space=space};

		struct InvaderKey : IEquatable<InvaderKey> {
			public Space Space;
			public Invader Invader;
			public bool Equals( InvaderKey other ) => Space==other.Space && Invader==other.Invader;
			public override bool Equals( object obj ) => Equals((InvaderKey)obj);
			public override int GetHashCode() => Space.GetHashCode() ^ Invader.GetHashCode();
			public InvaderKey ToHealthy() => new InvaderKey{ Space=Space, Invader=Invader.Healthy};
		}


		#endregion

		readonly CountDictionary<Space> blightCount = new CountDictionary<Space>();
		readonly CountDictionary<Space> beastCount = new CountDictionary<Space>();
		readonly CountDictionary<Space> wildsCount = new CountDictionary<Space>();

		readonly CountDictionary<InvaderKey> invaderCount = new CountDictionary<InvaderKey>();
		readonly CountDictionary<Space> dahanCount = new CountDictionary<Space>();

		readonly CountDictionary<Space> defendCount = new CountDictionary<Space>();
		public readonly HashSet<Space> noDamageToDahan = new(); // !!! special case for Conceiling Shadows - refactor!
		public int FearPool {get; private set; } = 0;
	}


}
