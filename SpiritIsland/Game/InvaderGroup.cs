using System;
using System.Collections.Generic;
using System.Linq;
using SpiritIsland;

namespace SpiritIsland {

	public class ReadOnlyInvaderGroup {

		#region static

		readonly static protected Dictionary<InvaderSpecific, int> idx;

		static ReadOnlyInvaderGroup() {
			idx = new Dictionary<InvaderSpecific, int>();
			int i = 0;
			idx.Add( InvaderSpecific.Explorer0, i++ );
			idx.Add( InvaderSpecific.Explorer, i++ );
			idx.Add( InvaderSpecific.Town0, i++ );
			idx.Add( InvaderSpecific.Town1, i++ );
			idx.Add( InvaderSpecific.Town, i++ );
			idx.Add( InvaderSpecific.City0, i++ );
			idx.Add( InvaderSpecific.City1, i++ );
			idx.Add( InvaderSpecific.City2, i++ );
			idx.Add( InvaderSpecific.City, i++ );
		}
		#endregion

		public ReadOnlyInvaderGroup( Space space, int[] counts ) {
			this.counts = counts; // for committing
			this.Space = space;
		}
		public Space Space { get; }

		public int this[InvaderSpecific specific] {
			get { return counts[idx[specific]]; }
			protected set { counts[idx[specific]] = value; }
		}

		public int this[Invader generic] => generic.AliveVariations.Sum( part => this[part] );

		public int TownsAndCitiesCount => this[Invader.Town] + this[Invader.City];

		public int TotalCount => counts.Sum(); // !!! if we add dahan to counts, this breaks

		/// <summary> Includes damaged invaders.</summary>
		public IEnumerable<InvaderSpecific> InvaderTypesPresent_Specific => idx.Keys.Where( invader => invader.Health > 0 && this[invader] > 0 );
		public IEnumerable<Invader> InvaderTypesPresent_Generic => InvaderTypesPresent_Specific.Select( x => x.Generic ).Distinct(); // not full healthy, but the full healthy equivalent of healty and damaged
		public InvaderSpecific[] FilterBy( params Invader[] healthyTypes ) => healthyTypes.SelectMany( x => x.AliveVariations ).Intersect( InvaderTypesPresent_Specific ).ToArray();

		public int DamageInflictedByInvaders => InvaderTypesPresent_Specific.Select( invader => invader.Healthy.Health * counts[idx[invader]] ).Sum();

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

		public override string ToString() {
			static int Order_CitiesTownsExplorers( InvaderSpecific invader )
				=> -(invader.Healthy.Health * 10 + invader.Health);
			return InvaderTypesPresent_Specific
				.Where( specific => this[specific] > 0 )
				.OrderBy( Order_CitiesTownsExplorers )
				.Select( invader => this[invader] + invader.Summary )
				.Join( "," );
		}

		#region private
		readonly int[] counts;
		#endregion

	}

	public class InvaderGroup : ReadOnlyInvaderGroup {

		#region constructor

		public InvaderGroup(Space space, int[] counts, Action<int> addFear )
			:base(space,counts)
		{
			this.addFear = addFear;
		}


		#endregion

		#region public Damage / Destory

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

		#endregion

		public void Adjust(InvaderSpecific invader,int delta ) {
			this[invader] = Math.Max(0,this[invader]+delta);
		}

		public void Heal() {

			void Heal( InvaderSpecific damaged ) {
				this[damaged.Healthy] += this[damaged];
				this[damaged] = 0;
			}

			Heal( InvaderSpecific.City1 );
			Heal( InvaderSpecific.City2 );
			Heal( InvaderSpecific.Town1 );
		}

		readonly Action<int> addFear;

	}

}
