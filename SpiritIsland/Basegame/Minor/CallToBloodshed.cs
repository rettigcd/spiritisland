using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class CallToBloodshed {

		[MinorCard("Call to Bloodshed",1,Speed.Slow,Element.Sun,Element.Fire,Element.Animal)]
		[FromPresence(2,Target.Dahan)]
		static public async Task Act(TargetSpaceCtx ctx){
			var target = ctx.Target;
			var (self,gameState)=ctx;

			if( await self.SelectFirstText( "Select option", "1 damage per dahan", "gather up to 3 dahan" ) )
				// opt 1 - 1 damage per dahan
				await ctx.DamageInvaders(target,gameState.GetDahanOnSpace(target));
			else
				// opt 2 - gather up to 3 dahan
				await ctx.GatherUpToNDahan(target,3);
		}
	}
}
