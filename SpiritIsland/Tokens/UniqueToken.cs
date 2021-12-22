
namespace SpiritIsland {

	public class UniqueToken : TokenClass, Token {

		public UniqueToken(string label, char initial, Img img, TokenCategory cat = TokenCategory.None) {
			this.Label = label;
			this.Img = img;
			this.Initial = initial;
			this.Category = cat;
		}


		#region Token

		public TokenClass Class => this;

		public Img Img { get; }

		public char Initial { get; }

		public string Summary => Initial.ToString();
		string IOption.Text => Summary;

		// --------  BEGIN Token HEALTH properties
		public Token Healthy => throw new System.NotImplementedException();
		public int FullHealth => throw new System.NotImplementedException();
		public int Health => 1; //throw new System.NotImplementedException();
		Token TokenClass.this[int i] => throw new System.NotImplementedException();
		Token Token.ResultingDamagedInvader( int damage ) => throw new System.NotImplementedException();

		// --------  END HEALTH properties  -------

		#endregion

		#region TokenGroup

		public Token Default => this;

		public string Label { get; }

		public TokenCategory Category { get; }

		// ------  Being TokenGroup HEALTH properties
		void TokenClass.ExtendHealthRange( int newMaxHealth ) => throw new System.NotImplementedException();
		// ------  End TokenGroup HEALTH properties


		#endregion

	}

}
