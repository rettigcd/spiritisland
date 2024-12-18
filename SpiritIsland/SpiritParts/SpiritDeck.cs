namespace SpiritIsland;

public class SpiritDeck {
	/// <remarks> Unused at present. anticipated future use.</remarks>
	public required DeckType Type { get; set; }
	public required List<PowerCard> Cards;
	public enum DeckType { Hand, InPlay, Discard, DaysThatNeverWere_Major, DaysThatNeverWere_Minor, Other  };
}
