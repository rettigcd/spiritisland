using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	class Gather1Beast : GrowthActionFactory {

		public override async Task ActivateAsync( SpiritGameStateCtx ctx ) {
			var to = await ctx.Self.TargetLandApi.TargetsSpace( ctx.Self, ctx.GameState, "Gather beast to", From.Presence, null, 2, Target.Any );
			await ctx.Target(to).GatherUpTo(1,TokenType.Beast.Generic);
		}

	}

}
