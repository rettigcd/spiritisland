using System;
using System.Collections.Generic;
using System.Linq;
using SpiritIsland;

namespace SpiritIsland {

	public class InvaderGroup_Readonly {

		#region static

		readonly static protected Dictionary<InvaderSpecific, int> idx;

		static InvaderGroup_Readonly() {
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

		public InvaderGroup_Readonly( Space space, int[] counts ) {
			this.counts = counts; // for committing
			this.Space = space;
		}
		public Space Space { get; }

		public int this[InvaderSpecific specific] {
			get { return counts[idx[specific]]; }
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

		public void Adjust( InvaderSpecific specific, int delta ) {
			counts[idx[specific]] = Math.Max( 0, this[specific] + delta );
			if(specific.Health == 0) {
				if(delta<=0) throw new ArgumentOutOfRangeException(nameof(delta),"should not be restoring dead invaders.");
			}
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

}
