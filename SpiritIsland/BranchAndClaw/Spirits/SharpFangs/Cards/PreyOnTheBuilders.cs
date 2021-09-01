using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class PreyOnTheBuilders {

		[SpiritCard( "Prey on the Builders", 1, Speed.Fast, Element.Moon, Element.Fire, Element.Animal )]
		[FromPresence(0)]
		public static Task ActAsync(TargetSpaceCtx ctx ) {
			if(ctx.GameState.BAC().Beasts.AreOn(ctx.Target))
				ctx.GameState.SkipBuild(ctx.Target);
			return Task.CompletedTask;
		}

	}

}
