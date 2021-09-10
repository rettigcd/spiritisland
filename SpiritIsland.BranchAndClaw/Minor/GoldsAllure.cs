using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class GoldsAllure {

		[MinorCard("Gold's Allure",0,Speed.Slow,Element.Fire,Element.Earth,Element.Animal)]
		[FromPresence(1,Target.Mountain)]
		static public Task ActAsync(TargetSpaceCtx ctx ) {
			ctx.GatherUpTo(1,Invader.Explorer);
			ctx.GatherUpTo(1,Invader.Town);
			return ctx.AddStrife();
		}

	}

}
