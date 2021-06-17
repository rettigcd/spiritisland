
using System;
using System.Collections.Generic;

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
		public void AddDahan( Space space ){ dahanCount[space]++; }
		public void AddCity( Space space ){ cityCount[space]++; }
		public void AddExplorer( Space space ){ explorerCount[space]++; }

		public void RemoveExplorer(Space space) { explorerCount[space]--; }

		public void AddTown( Space space ){ townCount[space]++; }
		public void RemoveTown(Space space) { townCount[space]--; }
		public void AddWilds( Space space ){ wildsCount[space]++; }

		public int GetDahanOnSpace( Space space ){ return dahanCount[space]; }

		public bool HasDahan( Space space ) => GetDahanOnSpace(space)>0;
		public bool HasInvaders( Space space ) => explorerCount[space]>0||townCount[space]>0||cityCount[space]>0;
		public bool HasWilds( Space s ) => wildsCount[s] > 0;
		public bool HasBlight( Space s ) => blightCount[s] > 0;
		public bool HasBeasts( Space s ) => beastCount[s] > 0;


		readonly DefaultDictionary<Space,int> blightCount = new DefaultDictionary<Space, int>();
		readonly DefaultDictionary<Space,int> beastCount = new DefaultDictionary<Space, int>();

		readonly DefaultDictionary<Space,int> cityCount = new DefaultDictionary<Space, int>();
		readonly DefaultDictionary<Space,int> townCount = new DefaultDictionary<Space, int>();
		readonly DefaultDictionary<Space,int> explorerCount = new DefaultDictionary<Space, int>();

		readonly DefaultDictionary<Space,int> dahanCount = new DefaultDictionary<Space, int>();

		readonly DefaultDictionary<Space,int> wildsCount = new DefaultDictionary<Space, int>();

		public InvaderGroup GetInvaderSummary(Space targetSpace) {
			return damaged ?? InitDamageDict(targetSpace);
		}

		InvaderGroup InitDamageDict(Space targetSpace) {
			var dict = new Dictionary<Invader, int> {
				[Invader.City] = cityCount[targetSpace],
				[Invader.Town] = townCount[targetSpace],
				[Invader.Explorer] = explorerCount[targetSpace]
			};
			return new InvaderGroup( dict );
		}

		internal void ApplyDamage(Space targetSpace, DamagePlan damagePlan) {
			damaged = InitDamageDict(targetSpace);
			damaged.ApplyDamage(damagePlan);
			this.cityCount[targetSpace] -= damaged.DestroyedCities;
			this.townCount[targetSpace] -= damaged.DestroyedTowns;
			this.explorerCount[targetSpace] -= damaged.DestroyedExplorers;
		}

		InvaderGroup damaged;

	}

}
