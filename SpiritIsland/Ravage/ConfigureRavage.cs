namespace SpiritIsland;

/// <summary>
/// Configures Dahan and Invader behavior on a per-space bases.
/// </summary>
public class ConfigureRavage {

	// Ravage - Specific
	public Func<RavageAction, Task> RavageSequence = null; // null triggers default
	public bool ShouldDamageLand { get; set; } = true;

	public CountDictionary<Token> NotParticipating {  get; set; } = new CountDictionary<Token>();
	public Func<HealthToken,bool> IsAttacker { get; set; } = null;
	public Func<HealthToken,bool> IsDefender { get; set; } = null;

	public int AttackersDefend = 0; // reduces the damage inflicted by the defenders onto the attackers.  Not exactly correct, but close

	// Dahan Behavior
	public bool ShouldDamageDahan { get; set; } = true;

}