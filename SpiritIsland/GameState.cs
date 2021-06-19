
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public class GameState {

		public GameState(params Spirit[] spirits){
			if(spirits.Length==0) throw new ArgumentException("Game must include at least 1 spirit");
			this.Spirits = spirits;
		}
		public Island Island { get; set; }
		public Spirit[] Spirits { get; }

		public void AddBeast( Space space ){ beastCount[space]++; }
		public void AddBlight( Space space ){ blightCount[space]++; }
		public void AddDahan( Space space, int delta=1 ){ dahanCount[space]+=delta; }

		public void AddWilds( Space space ){ wildsCount[space]++; }

		public int GetDahanOnSpace( Space space ){ return dahanCount[space]; }

		public bool HasDahan( Space space ) => GetDahanOnSpace(space)>0;
		public bool HasWilds( Space s ) => wildsCount[s] > 0;
		public bool HasBlight( Space s ) => blightCount[s] > 0;
		public bool HasBeasts( Space s ) => beastCount[s] > 0;


		readonly CountDictionary<Space> blightCount = new CountDictionary<Space>();
		readonly CountDictionary<Space> beastCount = new CountDictionary<Space>();
		readonly CountDictionary<Space> wildsCount = new CountDictionary<Space>();

		readonly CountDictionary<Space> dahanCount = new CountDictionary<Space>();

		#region Invaders

		readonly CountDictionary<InvaderKey> invaderCount = new CountDictionary<InvaderKey>();

		public bool HasInvaders( Space space ) 
			=> invaderCount.Keys.Any(k=>k.Space==space);

		public void Adjust(Invader invader, Space space, int count){
			invaderCount[Key(space,invader)] += count;
		}

		public InvaderGroup GetInvaderGroup(Space targetSpace) {
			return 
				// invaders ?? 
				InitInvaderGroup(targetSpace);
		}

		public void ApplyDamage(Space targetSpace, DamagePlan damagePlan) {
			var invaders = InitInvaderGroup(targetSpace);
			invaders.ApplyDamage(damagePlan);

			foreach(var invader in invaders.Changed)
				this.invaderCount[Key(targetSpace,invader)] = invaders[invader];
		}
		static InvaderKey Key(Space space,Invader invader) => new InvaderKey{ Invader=invader, Space=space};

		struct InvaderKey : IEquatable<InvaderKey> {
			public Space Space;
			public Invader Invader;
			public bool Equals( InvaderKey other ) => Space==other.Space && Invader==other.Invader;
			public override bool Equals( object obj ) => Equals((InvaderKey)obj);
			public override int GetHashCode() => Space.GetHashCode() ^ Invader.GetHashCode();
		}

		InvaderGroup InitInvaderGroup(Space targetSpace) {
			var dict1 = invaderCount.Keys
				.Where(k=>k.Space==targetSpace)
				.ToDictionary(k=>k.Invader,k=>invaderCount[k]);
			return new InvaderGroup( targetSpace, dict1 );
		}

//		InvaderGroup invaders;

		#endregion

	}

}
