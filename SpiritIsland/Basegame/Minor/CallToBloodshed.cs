using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class CallToBloodshed {

		[MinorCard("Call to Bloodshed",1,Speed.Slow,Element.Sun,Element.Fire,Element.Animal)]
		[FromPresence(2,Target.Dahan)]
		static public async Task Act(TargetSpaceCtx ctx){

			if( await ctx.Self.UserSelectsFirstText( "Select option", "1 damage per dahan", "gather up to 3 dahan" ) )
				// opt 1 - 1 damage per dahan
				await ctx.DamageInvaders(ctx.DahanCount);
			else
				// opt 2 - gather up to 3 dahan
				await ctx.GatherUpToNDahan(3);
		}
	}
}
