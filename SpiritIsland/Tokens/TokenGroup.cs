
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

		public void ExtendHealthRange(int newMaxHealth ) {
			if(newMaxHealth < seq.Length) return; // all good
			// replace old Seq with new extended seq
			var newSeq = new Token[newMaxHealth + 1];
			int h = 0;
			for(; h < seq.Length; ++h) newSeq[h] = seq[h];
			seq = newSeq;
			// Fill in the missing slots
			for(; h <= newMaxHealth; ++h)
				seq[h] = new Token( this, seq, h );
		}

		public Token this[int i] => seq[i];
		Token[] seq;

		public TokenGroup(string label, int fullHealth):this(label,fullHealth,label[0]) {}

		public TokenGroup( string label, int fullHealth, char initial ) {
			Label = label;
			Initial = initial;

			// Build different health level Sequence
			seq = BuildHealthSequence( this, fullHealth );

			// Capture default
			Default = seq[fullHealth];

		}

		public char Initial { get; }

		public string Label { get; }

		public Token Default { get; }

	}

}
