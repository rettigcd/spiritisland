
namespace SpiritIsland {

	public class TokenGroup {

		/// <summary>
		/// Create a set of Spefic invaders that have different health values
		/// </summary>
		static public Token[] BuildHealthSequence(TokenGroup generic, int fullHealth ) {
			var seq = new Token[fullHealth + 1];
			for(int h = 0; h <= fullHealth; ++h)
				seq[h] = new Token( generic, seq, h );
			return seq;
		}

		public Token this[int i] => seq[i];
		readonly Token[] seq;

		public TokenGroup(string label, int fullHealth) {
			Label = label;
			Initial = label[0];

			// Build different health level Sequence
			seq = BuildHealthSequence(this, fullHealth);

			// Capture default cast (????_
			Default = seq[^1];

		}

		public TokenGroup( string label, int fullHealth, char initial ) {
			Label = label;
			Initial = initial;

			// Build different health level Sequence
			seq = BuildHealthSequence( this, fullHealth );

			// Capture default cast (????_
			Default = seq[^1];

		}


		public char Initial { get; }

		public string Label { get; }

		public Token Default { get; }

	}

}
