using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class RegrowFromRoots {

		[SpiritCard("Regrow from Roots",1,Speed.Slow,Element.Water,Element.Plant)]
		[FromPresence(1,Target.JungleOrWetland)]
		static public Task ActAsync( TargetSpaceCtx ctx ) {

			// if ther are 2 blight or fewer in target land, remove 1 blight
			if( ctx.GameState.GetBlightOnSpace(ctx.Target) <= 2)
				ctx.GameState.AddBlight(ctx.Target,-1);

			return Task.CompletedTask;

		}

	}

}
