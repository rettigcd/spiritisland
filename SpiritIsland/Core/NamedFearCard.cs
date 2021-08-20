using System;

namespace SpiritIsland {
	/// <summary>
	/// Used to pu
	/// </summary>
	public class NamedFearCard : IOption {
		public IFearCard Card { get; set; }
		public string Text { get; set; }

		public string CardName => GetFearCardName(Card);

		static public string GetFearCardName( IFearCard card ) {
			Type type = card.GetType();
			return (string)type.GetField( "Name" ).GetValue( null );
		}

	}


	// TX the name to the UI so it can display the card for Bringer's innate
	public class DisplayFearCard : IOption {
		public string Text { get; set; }
	}


}
