
namespace SpiritIsland {

	public class TokenGroup {

		/// <summary>
		/// Create a set of Spefic invaders that have different health values
		/// </summary>
		static public Token[] BuildHealthSequence(TokenGroup generic, int fullHealth, Img[] imgs ) {
			var seq = new Token[fullHealth + 1];
			for(int h = 0; h <= fullHealth; ++h)
				seq[h] = new Token( generic, seq, h, h==0 ? Img.None : imgs[h-1] );
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
				seq[h] = new Token( this, seq, h, Default.Img );
		}

		public Token this[int i] => seq[i];
		Token[] seq;

		public TokenGroup(string label, int fullHealth, params Img[] imgs)
			:this(label,fullHealth,label[0], imgs) {}

		public TokenGroup( string label, int fullHealth, char initial, params Img[] imgs ) {
			Label = label;
			Initial = initial;

			// Build different health level Sequence
			seq = BuildHealthSequence( this, fullHealth, imgs );

			// Capture default
			Default = seq[fullHealth];

		}

		public char Initial { get; }

		public string Label { get; }

		public Token Default { get; }

	}

}
