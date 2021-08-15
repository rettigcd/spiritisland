using System;
using System.Collections.Generic;
using System.Linq;
using SpiritIsland;

namespace SpiritIsland {

	public class InvaderGroup{

		readonly static Dictionary<Invader,int> idx;

		static InvaderGroup() {
			idx = new Dictionary<Invader, int>();
			int i=0;
			idx.Add( Invader.Explorer0,i++);
			idx.Add( Invader.Explorer, i++ );
			idx.Add( Invader.Town0, i++ );
			idx.Add( Invader.Town1, i++ );
			idx.Add( Invader.Town, i++ );
			idx.Add( Invader.City0, i++ );
			idx.Add( Invader.City1, i++ );
			idx.Add( Invader.City2, i++ );
			idx.Add( Invader.City, i++ );
		}

		#region constructor

		public InvaderGroup(Space space, int[] counts, Action<int> addFear ) {

			this.counts = counts; // for committing
			this.Space = space;
			this.addFear = addFear;
		}


		#endregion

		public void ApplyDamageToEach( int individualDamage, params Invader[] healthyInvaders ) {
			foreach(var healthy in healthyInvaders)
				foreach(var part in healthy.AliveVariations)
					while(this[part] > 0)
						ApplyDamageTo1( individualDamage, part );
		}

		/// <returns>damage inflicted to invaders</returns>
		public int ApplyDamageTo1( int availableDamage, Invader invader ) {
			int damageToInvader = Math.Min( invader.Health, availableDamage );

			var damagedInvader = invader.Damage( damageToInvader );
			--counts[idx[invader]];

			++counts[idx[damagedInvader]];

			if(damagedInvader == Invader.City0)
				addFear( 2 );
			if(damagedInvader == Invader.Town0)
				addFear( 1 );

			return damageToInvader;
		}

		public int DestroyType( Invader healthy, int max ) => (max == 0) ? 0 : DestroyInner( healthy, max );

		int DestroyInner( Invader healthy, int countToDestory ) {
			Invader[] invaderTypesToDestory = this.InvaderTypesPresent
				.Where(i=>i.Healthy==healthy && i.Health!=0)
				.OrderByDescending(x=>x.Health) // kill healthiest first
				.ToArray();

			int totalDestoyed = 0;
			foreach(var invaderType in invaderTypesToDestory) {
				while(countToDestory>0 && counts[idx[invaderType]] > 0) {
					ApplyDamageTo1( invaderType.Health, invaderType );
					++totalDestoyed;
					--countToDestory;
				}
			}
			return totalDestoyed;
		}

		#region public Read-Only

		public int this[Invader i] => counts[idx[i]];

		public Space Space { get; }

		bool HasAnyKey(params Invader[] invader) => invader.Any(inv=>counts[idx[inv]]>0);

		public bool HasCity => HasAnyKey( Invader.City, Invader.City2, Invader.City1 );
		public bool HasTown => HasAnyKey( Invader.Town, Invader.Town1 );
		public bool HasExplorer => HasAnyKey( Invader.Explorer );

		public bool Has(params Invader[] healthyInvaders) => InvaderTypesPresent.Select(k=>k.Healthy).Intersect( healthyInvaders ).Any();

		/// <summary> Includes damaged invaders.</summary>
		public IEnumerable<Invader> InvaderTypesPresent => idx.Keys.Where(invader=>invader.Health>0 && counts[idx[invader]]>0);
		public IEnumerable<Invader> HealthyInvaderTypesPresent => InvaderTypesPresent.Select(x=>x.Healthy).Distinct(); // not full healthy, but the full healthy equivalent of healty and damaged
		public Invader[] FilterBy( params Invader[] healthyTypes ) => healthyTypes.SelectMany( x => x.AliveVariations ).Intersect( InvaderTypesPresent ).ToArray();

		public int TotalCount => counts.Sum();

		public int DamageInflictedByInvaders => InvaderTypesPresent.Select(invader=>invader.Healthy.Health * counts[idx[invader]] ).Sum();

		public int TypeCount( params Invader[] healthyInvaders ) => InvaderTypesPresent
			.Where( invader => healthyInvaders.Contains( invader.Healthy ) )
			.Sum( invader => counts[idx[invader]] );

		public override string ToString() {
			static int Order_CitiesTownsExplorers( Invader invader )
				=> -(invader.Healthy.Health * 10 + invader.Health);
			return InvaderTypesPresent
				.Where( invader => counts[idx[invader]] > 0 )
				.OrderBy( invader => Order_CitiesTownsExplorers( invader ) )
				.Select( invader => counts[idx[invader]] + invader.Summary )
				.Join( "," );
		}

		readonly static Invader[] damagedInvaders = new[] { Invader.City1, Invader.City2, Invader.Town1 };

		public void Heal() {
			foreach(var damaged in damagedInvaders) {
				counts[idx[damaged.Healthy]] += counts[idx[damaged]];
				counts[idx[damaged]] = 0;
			}
		}

		static public void Adjust(int[] counts, Invader invader, int delta ) {
			int index = idx[invader];
			counts[index] = Math.Max( 0, counts[index] + delta );
		}

		#endregion

		#region private
		readonly Action<int> addFear;
		readonly int[] counts;
		#endregion

	}

}
