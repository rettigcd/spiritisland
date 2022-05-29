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

	// Dahan Behavior
	// !!! Maybe this should be put on the DahanBinding object so it is effective outside ravage
	public Func<DahanGroupBinding,int,HealthToken,Task> DestroyDahan = DefaultDestroyDahan;
	public bool ShouldDamageDahan { get; set; } = true;
	static async Task DefaultDestroyDahan( DahanGroupBinding dahan, int count, HealthToken token ) {
		if(count<=0) return;
		await dahan.Destroy( count, token );
	}

}