namespace SpiritIsland {

//	public enum Track { None, Energy, Card };

	public class Track : IOption { 
		public static Track Energy = new Track("Energy"); 
		public static Track Card = new Track("Card");

		protected Track(string text){ this.Text = text; }

		public string Text {get;}
	}

}
