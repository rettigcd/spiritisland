namespace SpiritIsland;

public enum RemoveReason {
	None,
	Removed,    // Pulled from Game - Willingly
	Destroyed,  // Pulled from Game - Unwillingly
	Replaced,   // Pulled from Game - To be replaced with another
	UsedUp,     // Wilds, Disease, Strife
	MovedFrom   // Placed on another space.
	//	ReturnedToCard,    // Blight - nothing cares that blight is returned to card
}

static public class RemoveReasonExtension {
	static public bool IsDestroy( this RemoveReason reason ) => reason == RemoveReason.Destroyed;
}
