using System.Text;

namespace SpiritIsland {

	public class InvadersRavaged {

		public Space Space;

		public CountDictionary<Token> startingAttackers;
		public CountDictionary<Token> startingDefenders;
		public CountDictionary<Token> endingDefenders;
		public CountDictionary<Token> endingAttackers;

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
			buf.Append( $"Attackers ({startingAttackers.ToTokenSummary()}) dealt {defenderDamageFromAttackers} damage" );

			if(defenderDamageFromBadlands > 0)
				buf.Append($" plus {defenderDamageFromBadlands} badland damage");

			buf.Append( $" to defenders ({startingDefenders.ToTokenSummary()}) destroying {dahanDestroyed} of them." );

			// Defend Effect
			if(attackerDamageFromDefenders > 0) {
				buf.Append( $"  Defenders fight back dealing {attackerDamageFromDefenders} damage" );
				if(attackerDamageFromBadlands > 0)
					buf.Append($" plus {attackerDamageFromBadlands} badland damage");
				buf.Append( $", leaving {endingAttackers.ToTokenSummary()}." );
			}

			return buf.ToString();
		}

	}


}
