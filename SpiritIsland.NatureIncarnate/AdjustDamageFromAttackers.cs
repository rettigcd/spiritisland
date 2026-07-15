namespace SpiritIsland.NatureIncarnate;

/// <summary>
/// Configures Invaders(attackers) to do -6 damage
/// </summary>
public abstract class AdjustDamageFromAttackers
	: BaseModEntity, IConfigRavages, IEndWhenTimePasses, IAdjustAttackerDamage
{

	protected abstract int GetAdjustment( RavageExchange ravageExchange );

	public Task Config( Space st ) {
		var adjusters = st.RavageBehavior.DamageAdjusters;
		if( !adjusters.Contains( this ) )
			adjusters.Add( this );
		return Task.CompletedTask;
	}

	public int Adjust( RavageExchange ravageExchange, int runningTotal )
		=> Math.Max( 0, runningTotal + GetAdjustment( ravageExchange ) );
}

