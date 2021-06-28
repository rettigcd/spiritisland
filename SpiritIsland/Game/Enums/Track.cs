namespace SpiritIsland {

	public class Track : IOption { 
		public static readonly Track Energy = new Track("Energy"); 
		public static readonly Track Card = new Track("Card");

		protected Track(string text){ this.Text = text; }

		public string Text {get;}
	}

}
