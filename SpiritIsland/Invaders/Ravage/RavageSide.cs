namespace SpiritIsland.Invaders.Ravage;

/// <summary>
/// Is the token an attacker, defender, or non-participant.
/// </summary>
public enum RavageSide {
	None, // not participating
	Attacker, // damages land and defenders
	Defender, // does not damage land, damages attackers
}
