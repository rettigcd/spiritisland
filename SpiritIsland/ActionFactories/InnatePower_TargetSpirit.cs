using System;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class InnatePower_TargetSpirit : InnatePower, IActionFactory {

		#region Constructors and factories

		internal InnatePower_TargetSpirit( Type type ):base(type,LandOrSpirit.Spirit) {}

		#endregion

		public override async Task ActivateAsync( Spirit self, GameState gameState ) {
			Spirit target = gameState.Spirits.Length == 1
				? gameState.Spirits[0]
				: await self.Action.Decision(new Decision.TargetSpirit(gameState.Spirits));

			await PowerCard_TargetSpirit.TargetSpirit( HighestMethod( self ), self, gameState, target );
		}

		public override string TargetFilter => "Spirit";

		public override string RangeText => "-";
	}

}