using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {
	public class InvaderGroup{

		public InvaderGroup(Dictionary<Invader,int> dict){
			this.dict = dict;
			dd = new CountDictionary<Invader>(dict);
		}

		readonly Dictionary<Invader,int> dict;
		readonly CountDictionary<Invader> dd;

		public override string ToString() {
			return dict
				.Where(pair => pair.Value > 0 && pair.Key.Health>0)
				.OrderBy(pair => pair.Key.Order)
				.Select(p => p.Value + p.Key.Summary)
				.Join(",");
		}

		public IEnumerable<Invader> InvaderTypesPresent => dict
			.Where(pair => pair.Value > 0 && pair.Key.Health>0)
			.Select(pair => pair.Key);

		public void ApplyDamage(DamagePlan damagePlan) {
			// remove Healthy
			--dd[damagePlan.Invader];
			// add damaged
			var resultInvader = damagePlan.Invader.Damage(damagePlan.Damage);
			++dd[resultInvader];
		}

		public bool HasCity => dict.ContainsKey(Invader.City);
		public bool HasTown => dict.ContainsKey(Invader.Town);
		public bool HasExplorer => dict.ContainsKey(Invader.Explorer);

		public int DestroyedCities => dd[Invader.City0];
		public int DestroyedTowns => dd[Invader.Town0];
		public int DestroyedExplorers => dd[Invader.Explorer0];
	}

}
