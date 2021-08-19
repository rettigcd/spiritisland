using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class LandOfHauntsAndEmbers {

		[MinorCard("Land of Haunts and Embers",0,Speed.Fast,Element.Moon,Element.Fire,Element.Air)]
		[FromPresence(2)]
		static public async Task Act(TargetSpaceCtx ctx){

			// 2 fear
			ctx.AddFear(2);

			bool hasBlight = ctx.HasBlight;
			// if target has blight
			if(hasBlight){
				// +2 fear
				ctx.AddFear(2);
				// add 1 blight
				ctx.AddBlight();
			}

			// push up to 2 more explorers/towns, add 1 blight
			int pushCount = hasBlight ? 4 : 2;
			await ctx.PushUpToNInvaders(pushCount,Invader.Explorer,Invader.Town);
		}

	}
}
