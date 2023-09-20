namespace SpiritIsland;

/// <summary> The action we are taking with this card. </summary>
public enum CardUse { 
	AddToHand, 
	Discard, // This is for Events
	Forget, 
	Gift, 
	Play, 
	Reclaim, 
	Repeat, 
	Other
};