﻿namespace SpiritIsland.Core {
	public class TextOption : IOption {

		static public readonly TextOption Done = new TextOption("Done");

		public TextOption(string text){ Text = text; }
		public string Text { get; }

		public bool Matches(IOption option ){
			var match = option is TextOption txt && txt.Text == Text;
			if( match )
				return true;
			return false;
		}
	}


}
