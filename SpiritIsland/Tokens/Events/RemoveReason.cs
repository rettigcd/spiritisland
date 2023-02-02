namespace SpiritIsland;

public enum RemoveReason {
	None,
	Removed,           // Generic 'Remove X' command
	UsedUp,            // Wilds, Disease, Strife
	Destroyed,
	Replaced,          // with another token
//	ReturnedToCard,    // Blight - nothing cares that blight is returned to card
	MovedFrom
}

static public class RemoveReasonExtension {
	static public bool IsDestroy( this RemoveReason reason ) => reason == RemoveReason.Destroyed;
}
