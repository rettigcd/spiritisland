namespace SpiritIsland;

public class RavageParticipants(
	CountDictionary<HumanToken> starting,
	CountDictionary<HumanToken> active
) {

	/// <summary> Anyone that can receive damage. </summary>
	readonly public CountDictionary<HumanToken> Starting = starting;

	/// <summary> The participants that deal damage. </summary>
	readonly public CountDictionary<HumanToken> Active = active;

	/// <summary> Damage dealt (EXCLUDING Badland Damage) out to the other side. (less any Defend) </summary>
	public int DamageDealtOut;

	public int DamageReceivedFromBadlands;

	public CountDictionary<HumanToken>? Ending;

}
