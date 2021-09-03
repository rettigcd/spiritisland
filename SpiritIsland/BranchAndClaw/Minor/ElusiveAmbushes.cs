using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class ElusiveAmbushes {

		[MinorCard("Elusive Ambushes",1,Speed.Fast,Element.Sun,Element.Fire,Element.Water)]
		[FromPresence(1,Target.Dahan)]
		static public async Task ActAsync(TargetSpaceCtx ctx ) {
			if(await ctx.Self.UserSelectsFirstText("Choose","1 damage","Defend 4"))
				ctx.DamageInvaders(1);
			else
				ctx.Defend(4);
		}

	}


}
