using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class LandOfHauntsAndEmbers {

		[MinorCard("Land of Haunts and Embers",0,Speed.Fast,Element.Moon,Element.Fire,Element.Air)]
		[FromPresence(2)]
		static public async Task Act(TargetSpaceCtx ctx){
			var target = ctx.Target;

			// 2 fear
			ctx.AddFear(2);

			bool hasBlight = ctx.GameState.HasBlight(target);
			// if target has blight
			if(hasBlight){
				// +2 fear
				ctx.AddFear(2);
				// add 1 blight
				ctx.GameState.AddBlight(target,1);
			}

			// push up to 2 more explorers/towns, add 1 blight
			int pushCount = hasBlight ? 4 : 2;
			await ctx.PushUpToNInvaders(target,pushCount,Invader.Explorer,Invader.Town);
		}

	}
}
