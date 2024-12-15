namespace SpiritIsland;

public class SpiritDeck {
	/// <remarks> Unused at present. anticipated future use.</remarks>
	public DeckType Type { get; set; }
	public List<PowerCard> Cards;
	public enum DeckType { Hand, InPlay, Discard, DaysThatNeverWere_Major, DaysThatNeverWere_Minor, Other  };
}
