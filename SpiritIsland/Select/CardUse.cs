namespace SpiritIsland;

/// <summary> The action we are taking with this card. </summary>
public enum CardUse {
	None,
	AddToHand, 
	Discard, // This is for Events
	Forget, 
	Gift, 
	Play,
	Impend,
	Reclaim, 
	Repeat, 
	Accept   // Generic card use. (was "Other")
};