namespace SpiritIsland;

public enum RemoveReason {
	None,
	Removed,    // Pulled from Game - Willingly
	Destroyed,  // Pulled from Game - Unwillingly
	Replaced,   // Pulled from Game - To be replaced with another
	UsedUp,     // Wilds, Disease, Strife
	MovedFrom,  // Placed on another space.
	TakingFromCard, // Stops BlightToken.HandleRemoved from putting it back on the card.
	Abducted,		// Unwillingly moved to Endless Dark
}

static public class RemoveReasonExtension {
	static public bool IsDestroy( this RemoveReason reason ) => reason == RemoveReason.Destroyed;
}


/// <summary>
/// Token is destroyed by receiving Direct Damage.
/// </summary>
/// <remarks>
/// Hook for Habsburg Monarchy Durable Towns.
/// Not part of the interface because we don't want it in the Move event
/// </remarks>
public class DestroyingFromDamage( Space from ) : RemovingTokenArgs( from, RemoveReason.Destroyed ) {

	/// <summary>
	/// Special value used to Trigger this Type 
	/// </summary>
	static readonly public RemoveReason TriggerReason = (RemoveReason)151;
}

