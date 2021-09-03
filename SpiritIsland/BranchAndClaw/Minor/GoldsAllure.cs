using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class GoldsAllure {

		[MinorCard("Gold's Allure",0,Speed.Slow,Element.Fire,Element.Earth,Element.Animal)]
		[FromPresence(1,Target.Mountain)]
		static public Task ActAsync(TargetSpaceCtx ctx ) {
			ctx.GatherUpToNTokens(1,Invader.Explorer);
			ctx.GatherUpToNTokens(1,Invader.Town);
			return ctx.Self.SelectInvader_ToStrife(ctx.Tokens);
		}

	}

}
