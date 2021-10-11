using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class ElusiveAmbushes {

		[MinorCard("Elusive Ambushes",1,Element.Sun,Element.Fire,Element.Water)]
		[Fast]
		[FromPresence(1,Target.Dahan)]
		static public Task ActAsync(TargetSpaceCtx ctx ) {

			return ctx.SelectActionOption(
				new ActionOption("1 damage", ()=>ctx.DamageInvaders(1)),
				new ActionOption("Defend 4", () => ctx.Defend(4))
			);

		}

	}


}
