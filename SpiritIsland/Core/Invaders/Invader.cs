using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public class Invader {

		static readonly public Invader Explorer = new Invader( "Explorer", 1 );
		static readonly public Invader Town = new Invader( "Town", 2 );
		static readonly public Invader City = new Invader( "City", 3 );

		/// <summary>
		/// Create a set of Spefic invaders that have different health values
		/// </summary>
		static public InvaderSpecific[] BuildHealthSequence(Invader generic, int fullHealth ) {
			var seq = new InvaderSpecific[fullHealth + 1];
			for(int h = 0; h <= fullHealth; ++h)
				seq[h] = new InvaderSpecific( generic, seq, h );
			return seq;
		}

		public InvaderSpecific this[int i] => seq[i];
		readonly InvaderSpecific[] seq;

		public Invader(string label, int fullHealth) {
			alive = new List<InvaderSpecific>();
			Label = label;

			// Build different health level Sequence
			seq = BuildHealthSequence(this, fullHealth);

			// Capture default cast (????_
			Healthy = seq[^1]; // for implicit type casting ... is this still correct?

		}

		public string Label { get; }

		readonly List<InvaderSpecific> alive;

		public InvaderSpecific Healthy { get; }

	}

}
