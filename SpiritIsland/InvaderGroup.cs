using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public class InvaderGroup{

		public Space Space { get; }

		public InvaderGroup(Space space,Dictionary<Invader,int> dict){
			this.Space = space;
			this.dict = dict;
			dd = new CountDictionary<Invader>(dict);
		}

		readonly Dictionary<Invader,int> dict;
		readonly CountDictionary<Invader> dd;

		public override string ToString() {
			return dict
				.Where(pair => pair.Value > 0 && pair.Key.Health>0)
				.OrderBy(pair => CitiesTownsExplorers(pair.Key))
				.Select(p => p.Value + p.Key.Summary)
				.Join(",");
		}
		static int CitiesTownsExplorers(Invader invader)
			=> -(invader.Healthy.Health * 10 + invader.Health);

		/// <summary> Includes damaged invaders.</summary>
		public IEnumerable<Invader> InvaderTypesPresent => dict
			.Where(pair => pair.Value > 0 && pair.Key.Health>0)
			.Select(pair => pair.Key);

		public void ApplyDamage(DamagePlan damagePlan) {
			changed.Add( damagePlan.Invader );			--dd[ damagePlan.Invader ];
			changed.Add( damagePlan.DamagedInvader );	++dd[ damagePlan.DamagedInvader ];
		}

		public int this[Invader i] {
			get{ return dd[i]; }
			set{  dd[i] = value; }
		}

		public bool HasCity => dict.ContainsKey(Invader.City);
		public bool HasTown => dict.ContainsKey(Invader.Town) || dict.ContainsKey(Invader.Town1);
		public bool HasExplorer => dict.ContainsKey(Invader.Explorer);

		public int DestroyedCities => dd[Invader.City0];
		public int DestroyedTowns => dd[Invader.Town0];
		public int DestroyedExplorers => dd[Invader.Explorer0];

		readonly HashSet<Invader> changed = new HashSet<Invader>();
		public IEnumerable<Invader> Changed => changed.Where(i=>i.Health != 0);

	}

}
