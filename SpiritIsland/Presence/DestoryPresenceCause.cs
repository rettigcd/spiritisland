namespace SpiritIsland;

public enum DestoryPresenceCause {
	None,
	SpiritPower,    // One use of a Power;
	DahanDestroyed, // Thunderspeaker
	SkipInvaderAction, // A Spread of Rampant Green

	Blight,          // blight added to land
	BlightedIsland,  // Direct effect of the Blighted Island card


	// Not used
	Adversary,      //An Adversary's Escalation effect
	Event,          // Everything a Main/Token/Dahan Event does
	FearCard,       // Everything one Fear Card does
}
