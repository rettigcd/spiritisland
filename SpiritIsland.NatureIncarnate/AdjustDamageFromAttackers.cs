namespace SpiritIsland.NatureIncarnate;

/// <summary>
/// Configures Invaders(attackers) to do -6 damage
/// </summary>
public abstract class AdjustDamageFromAttackers
	: BaseModEntity, IConfigRavages, IEndWhenTimePasses, IAdjustAttackerDamage, ISerializableSpaceEntity
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

	// Both current subclasses are parameterless, so one shared implementation (tagged with the
	// concrete runtime type, same trick as SpiritPresenceToken.ToJson) covers the whole family -
	// each subclass only needs its own [ModuleInitializer] reader registration.
	public virtual JsonArray ToJson( ISerializationContext ctx ) => new JsonArray( GetType().Name );
}

