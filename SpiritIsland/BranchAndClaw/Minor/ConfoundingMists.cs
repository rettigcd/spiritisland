using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class ConfoundingMists {

		[MinorCard( "Confounding Mists", 1, Speed.Fast, Element.Air, Element.Water )]
		[FromPresence( 1 )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {
			return ctx.SelectPowerOption(
				new PowerOption("Defend 4", ctx=>ctx.Defend(4)),
				new PowerOption("Invaders added to target are immediately pushed", PushFutureInvadersFromLans)
			);
		}

		static Task PushFutureInvadersFromLans( TargetSpaceCtx ctx ) {

			// each invader added to target land this turn may be immediatley pushed to any adjacent land
			// !!!

			return Task.CompletedTask;
		}
	}

}
