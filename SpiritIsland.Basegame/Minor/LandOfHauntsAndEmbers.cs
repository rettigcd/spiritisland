using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class LandOfHauntsAndEmbers {

		[MinorCard("Land of Haunts and Embers",0,Speed.Fast,Element.Moon,Element.Fire,Element.Air)]
		[FromPresence(2)]
		static public async Task Act(TargetSpaceCtx ctx){

			int pushCount = 2;

			// 2 fear
			ctx.AddFear(2);

			// if target has blight
			if(ctx.HasBlight) {
				// +2 fear
				ctx.AddFear(2);
				// add 1 blight
				ctx.AddBlight();
				// push up to 2 more explorers/towns, add 1 blight
				++pushCount;
			}

			await ctx.PushUpToNTokens(pushCount,Invader.Explorer,Invader.Town);
		}

	}
}
