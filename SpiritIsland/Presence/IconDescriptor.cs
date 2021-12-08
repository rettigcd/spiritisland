namespace SpiritIsland {

	public class IconDescriptor {

		public IconDescriptor Super;

		public string BackgroundImg;

		public string ContentImg;
		public string Text;
		public string ContentImg2;

		public IconDescriptor Sub;

	}

	public class ImageNames {
		public const string Coin          = "tokens.coin.png";
		public const string CardPlay      = "icons.cardplay.png";
		public const string Reclaim1      = "icons.reclaim 1.png";
		public const string Plus1CardPlay = "icons.Cardplayplusone.png";
		public const string Minor         = "icons.minor.png";
		public const string PrepareEl     = "icons.PrepareElement.png";
		public const string Discard2PrepForCardPlay = "icons.DiscardElementsForCardPlay.png";
		public const string Push1dahan    = "icons.Push1dahan.png";
		public const string GainCard      = "icons.GainCard.png";
		public const string Movepresence  = "icons.MovePresence.png";
		static public string For(Element element) => "tokens.Simple_"+element.ToString().ToLower()+".png";
	}

}
