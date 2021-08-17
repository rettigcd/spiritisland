using System;
using System.Collections.Generic;
using System.Linq;
using SpiritIsland;

namespace SpiritIsland {

	public class InvaderGroup{

		readonly static Dictionary<InvaderSpecific,int> idx;

		static InvaderGroup() {
			idx = new Dictionary<InvaderSpecific, int>();
			int i=0;
			idx.Add( InvaderSpecific.Explorer0,i++);
			idx.Add( InvaderSpecific.Explorer, i++ );
			idx.Add( InvaderSpecific.Town0, i++ );
			idx.Add( InvaderSpecific.Town1, i++ );
			idx.Add( InvaderSpecific.Town, i++ );
			idx.Add( InvaderSpecific.City0, i++ );
			idx.Add( InvaderSpecific.City1, i++ );
			idx.Add( InvaderSpecific.City2, i++ );
			idx.Add( InvaderSpecific.City, i++ );
		}

		#region constructor

		public InvaderGroup(Space space, int[] counts, Action<int> addFear ) {

			this.counts = counts; // for committing
			this.Space = space;
			this.addFear = addFear;
		}


		#endregion

		public void ApplyDamageToEach( int individualDamage, params Invader[] invaders ) {
			foreach(Invader invader in invaders)
				foreach(var part in invader.AliveVariations)
					while(this[part] > 0)
						ApplyDamageTo1( individualDamage, part );
		}

		/// <returns>damage inflicted to invaders</returns>
		public int ApplyDamageTo1( int availableDamage, InvaderSpecific invader ) {
			int damageToInvader = Math.Min( invader.Health, availableDamage );

			var damagedInvader = invader.Damage( damageToInvader );
			--this[invader];

			++this[damagedInvader];

			if(damagedInvader == InvaderSpecific.City0)
				addFear( 2 );
			if(damagedInvader == InvaderSpecific.Town0)
				addFear( 1 );

			return damageToInvader;
		}

		public int Destroy( Invader healthy, int max ) => (max == 0) ? 0 : DestroyInner( healthy, max );

		int DestroyInner( Invader generic, int countToDestory ) {
			InvaderSpecific[] invaderTypesToDestory = generic.AliveVariations
				.Where(specific => this[specific]>0)
				.OrderByDescending(x=>x.Health) // kill healthiest first
				.ToArray();

			int totalDestoyed = 0;
			foreach(var specificInvader in invaderTypesToDestory) {
				while(countToDestory>0 && this[specificInvader] > 0) {
					ApplyDamageTo1( specificInvader.Health, specificInvader );
					++totalDestoyed;
					--countToDestory;
				}
			}
			return totalDestoyed;
		}

		public void Adjust(InvaderSpecific invader,int delta ) {
			this[invader] = Math.Max(0,this[invader]+delta);
		}

		static public void Adjust( int[] counts, InvaderSpecific invader, int delta ) {
			int index = idx[invader];
			counts[index] = Math.Max( 0, counts[index] + delta );
		}

		public void Heal() {
			foreach(var damaged in damagedInvaders) {
				this[damaged.Healthy] += this[damaged];
				this[damaged] = 0;
			}
		}

		#region Counts

		public int this[InvaderSpecific specific] {
			get{ return counts[idx[specific]]; }
			private set { counts[idx[specific]]=value; }
		}

		public int this[Invader generic] => generic.AliveVariations.Sum(part=>this[part]);

		public int TownsAndCitiesCount => this[Invader.Town] + this[Invader.City];

		public int TotalCount => counts.Sum(); // !!! if we add dahan to counts, this breaks

		#endregion

		#region public Read-Only

		public Space Space { get; }

		//bool HasAnyKey(params InvaderSpecific[] invader) => invader.Any(inv=>this[inv]>0);

		public bool HasCity => Has( Invader.City );
		public bool HasTown => Has( Invader.Town );
		public bool HasExplorer => Has( Invader.Explorer );
		public bool HasAny( params Invader[] healthyInvaders ) => healthyInvaders.Any( Has );
		public bool Has( Invader inv ) => inv.AliveVariations.Any( alive => this[alive] > 0 );

		public InvaderSpecific PickBestInvaderToRemove( params Invader[] removable ) {
			return removable.SelectMany( generic => generic.Specifics )
				.Where( generic => this[generic] > 0 )
				.OrderByDescending( g => g.Generic.Healthy.Health )
				.ThenByDescending( g => g.Health )
				.First();
		}

		/// <summary> Includes damaged invaders.</summary>
		public IEnumerable<InvaderSpecific> InvaderTypesPresent_Specific => idx.Keys.Where(invader=>invader.Health>0 && this[invader]>0);
		public IEnumerable<Invader> InvaderTypesPresent_Generic => InvaderTypesPresent_Specific.Select(x=>x.Generic).Distinct(); // not full healthy, but the full healthy equivalent of healty and damaged
		public InvaderSpecific[] FilterBy( params Invader[] healthyTypes ) => healthyTypes.SelectMany( x => x.AliveVariations ).Intersect( InvaderTypesPresent_Specific ).ToArray();

		public int DamageInflictedByInvaders => InvaderTypesPresent_Specific.Select(invader=>invader.Healthy.Health * counts[idx[invader]] ).Sum();

		public override string ToString() {
			static int Order_CitiesTownsExplorers( InvaderSpecific invader )
				=> -(invader.Healthy.Health * 10 + invader.Health);
			return InvaderTypesPresent_Specific
				.Where( invader => counts[idx[invader]] > 0 )
				.OrderBy( invader => Order_CitiesTownsExplorers( invader ) )
				.Select( invader => counts[idx[invader]] + invader.Summary )
				.Join( "," );
		}

		readonly static InvaderSpecific[] damagedInvaders = new[] { InvaderSpecific.City1, InvaderSpecific.City2, InvaderSpecific.Town1 };

		#endregion

		#region private
		readonly Action<int> addFear;
		readonly int[] counts;
		#endregion

	}

}
