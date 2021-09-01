using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public class InvaderCounts : IInvaderCounts {

		#region static 

		public const int TypesCount = 6;

		#endregion

		#region constructor

		public InvaderCounts( CountDictionary<InvaderSpecific> counts ) { this.counts = counts; }

		#endregion

		public int this[InvaderSpecific specific] {
			get {
				ValidateIsAlive( specific );
				return counts[specific];
			}
			set {
				ValidateIsAlive( specific );
				counts[specific] = value; 
			}
		}

		public IEnumerable<InvaderSpecific> Keys => counts.Keys;

		public int Total => counts.Values.Sum();

		#region private
		static void ValidateIsAlive( InvaderSpecific specific ) {
			if(specific.Health == 0) 
				throw new ArgumentException( "We don't store dead counts" );
		}

		readonly CountDictionary<InvaderSpecific> counts;

		#endregion

	}

	public class ArrayInvaderCounts : IInvaderCounts {

		#region static 

		public const int TypesCount = 6;

		readonly static protected Dictionary<InvaderSpecific, int> idx;

		static ArrayInvaderCounts() {
			idx = new Dictionary<InvaderSpecific, int>();
			int i = 0;
			idx.Add( Invader.Explorer[1], i++ );
			idx.Add( Invader.Town[1], i++ );
			idx.Add( Invader.Town[2], i++ );
			idx.Add( Invader.City[1], i++ );
			idx.Add( Invader.City[2], i++ );
			idx.Add( Invader.City[3], i++ );
		}

		#endregion

		#region constructor

		public ArrayInvaderCounts( int[] counts ) { this.counts = counts; }

		#endregion

		public int this[InvaderSpecific specific] {
			get {
				ValidateIsAlive( specific );
				return counts[idx[specific]];
			}
			set {
				ValidateIsAlive( specific );
				counts[idx[specific]] = value;
			}
		}

		public IEnumerable<InvaderSpecific> Keys => idx.Keys.Where( invader => this[invader] > 0 );

		public int Total => counts.Sum();

		#region private
		static void ValidateIsAlive( InvaderSpecific specific ) {
			if(specific.Health == 0)
				throw new ArgumentException( "We don't store dead counts" );
		}

		readonly int[] counts;

		#endregion

	}


}
