namespace SpiritIsland.NatureIncarnate;

/// <summary>
/// Configures Invaders(attackers) to do -6 damage
/// </summary>
class AdjustDamageFromAttackers( Func<RavageExchange, int> damageAdjustment ) 
	: BaseModEntity, IConfigRavages, IEndWhenTimePasses
{

	readonly Func<RavageExchange,int> _damageAdjustment = damageAdjustment;

	public Task Config( Space st ) {
		// ??? Could the ConfigureRavage handlers just mod the RavageBehavior?
		RavageBehavior behavior = st.RavageBehavior;
		Func<RavageExchange, int> old = st.RavageBehavior.GetDamageFromParticipatingAttackers;
		st.RavageBehavior.GetDamageFromParticipatingAttackers = (ravageExchange) => {
			int originalDamage = old(ravageExchange);
			int adjustment = _damageAdjustment(ravageExchange); 
			return Math.Max(0,originalDamage + adjustment);
		};
		return Task.CompletedTask;
	}
}

