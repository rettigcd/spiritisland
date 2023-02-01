namespace SpiritIsland;

public class InvadersRavaged {

	public Space Space;

	public CountDictionary<HumanToken> startingAttackers;
	public CountDictionary<HumanToken> startingDefenders;
	public CountDictionary<HumanToken> endingDefenders;
	public CountDictionary<HumanToken> endingAttackers;

	public int defend;

	public int defenderDamageFromAttackers; // post-defend.  Defend points already applied
	public int defenderDamageFromBadlands;  // post-defend.  Defend points already applied
	public int attackerDamageFromDefenders;
	public int attackerDamageFromBadlands;

	public int dahanDestroyed;

	public override string ToString() {
		var buf = new StringBuilder();

		buf.Append( $"{Space.Label}: " );

		if(defend>0)
			buf.Append( $"Defend {defend}. " );

		// Attacker Effect
		buf.Append( $"Attackers ({startingAttackers.TokenSummary()}) dealt {defenderDamageFromAttackers} damage" );

		if(defenderDamageFromBadlands > 0)
			buf.Append($" plus {defenderDamageFromBadlands} badland damage");

		buf.Append( $" to defenders ({startingDefenders.TokenSummary()}) destroying {dahanDestroyed} of them." );

		// Defend Effect
		if(attackerDamageFromDefenders > 0) {
			buf.Append( $"  Defenders fight back dealing {attackerDamageFromDefenders} damage" );
			if(attackerDamageFromBadlands > 0)
				buf.Append($" plus {attackerDamageFromBadlands} badland damage");
			buf.Append( $", leaving {endingAttackers.TokenSummary()}." );
		}

		return buf.ToString();
	}

}