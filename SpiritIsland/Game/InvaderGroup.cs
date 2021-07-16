using System;
using System.Collections.Generic;
using System.Linq;
using SpiritIsland.Core;

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

		public Invader[] FilterBy(params Invader[] allowedTypes ){
			return InvaderTypesPresent
				.Where(i=>allowedTypes.Contains(i.Healthy))
				.ToArray();
		}


		public void ApplyDamage(DamagePlan damagePlan) {
			changed.Add( damagePlan.Invader );			--dd[ damagePlan.Invader ];
			changed.Add( damagePlan.DamagedInvader );	++dd[ damagePlan.DamagedInvader ];
		}

		public int this[Invader i] {
			get{ return dd[i]; }
			set{ 
				changed.Add(i); // so value gets persisted to gamestate
				dd[i] = value;
			}
		}

		public bool HasCity => dict.ContainsKey(Invader.City) || dict.ContainsKey(Invader.City2) || dict.ContainsKey(Invader.City1);
		public bool HasTown => dict.ContainsKey(Invader.Town) || dict.ContainsKey(Invader.Town1);
		public bool HasExplorer => dict.ContainsKey(Invader.Explorer);

		public int DestroyedCities => dd[Invader.City0];
		public int DestroyedTowns => dd[Invader.Town0];
		public int DestroyedExplorers => dd[Invader.Explorer0];

		readonly HashSet<Invader> changed = new HashSet<Invader>();
		public IEnumerable<Invader> Changed => changed.Where(i=>i.Health != 0);

		public int Destroy( Invader healthy, int? max = null ) {
			var invadersToDestory = this.InvaderTypesPresent
				.Where(i=>i.Healthy==healthy)
				.OrderByDescending(x=>x.Health) // kill healthiest first
				.ToArray();
			if(max.HasValue)
				invadersToDestory = invadersToDestory.Take(max.Value).ToArray(); 
			int total = 0;
			foreach(var key in invadersToDestory){
				int count = this[key];
				total += count;
				this[key.Dead] += count;
				this[key] = 0;
			}
			return total;
		}


		public int TotalCount { get {
			int total = 0;
			foreach(var k in InvaderTypesPresent)
				total += this[k];
			return total;
		} }

		public int DamageInflicted => dict
			.Select(p=>p.Value * p.Key.Healthy.Health)
			.Sum();
	}

}
