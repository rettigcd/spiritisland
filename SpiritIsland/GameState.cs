
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public class GameState {

		public Island Island { get; set; }

		public void AddBeast( Space space ){ beastCount[space]++; }
		public void AddBlight( Space space ){ blightCount[space]++; }
		public void AddDahan( Space space ){ dahanCount[space]++; }
		public void AddCity( Space space ){ cityCount[space]++; }
		public void AddExplorer( Space space ){ explorerCount[space]++; }
		public void AddTown( Space space ){ townCount[space]++; }
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

		public string GetInvaderSummary(Space targetSpace) {

			return (damaged ?? InitDamageDict(targetSpace))
				.OrderBy(pair => pair.Key switch {
					"C@3" => 0,
					"C@2" => 1,
					"C@1" => 2,
					"T@2" => 3,
					"T@1" => 4,
					"E@1" => 5,
					_ => 6
				})
				.Where(pair => pair.Value > 0)
				.Select(p => p.Value + p.Key)
				.Join(",");
		}

		Dictionary<string, int> InitDamageDict(Space targetSpace) {
			return new Dictionary<string, int> {
				["C@3"] = cityCount[targetSpace],
				["T@2"] = townCount[targetSpace],
				["E@1"] = explorerCount[targetSpace]
			};
		}

		internal void ApplyDamage(Space targetSpace, DamagePlan damagePlan) {
			damaged = InitDamageDict(targetSpace);
			var d = new DefaultDictionary<string,int>(damaged);
			--d[damagePlan.InvaderHealth];
			switch(damagePlan.ToString()){
				case "1>C@3": d["C@2"]++; break;
				case "2>C@3": d["C@1"]++; break;
				case "1>T@2": d["T@1"]++; break;
				case "3>C@3": --this.cityCount[targetSpace]; break;
				case "2>T@2": --this.townCount[targetSpace]; break;
				case "1>E@1": --this.explorerCount[targetSpace]; break;
			}

		}

		Dictionary<string,int> damaged;

	}

}
