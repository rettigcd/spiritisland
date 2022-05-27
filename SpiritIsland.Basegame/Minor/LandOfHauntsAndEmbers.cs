namespace SpiritIsland.Basegame;

public class LandOfHauntsAndEmbers {

	[MinorCard("Land of Haunts and Embers",0,Element.Moon,Element.Fire,Element.Air)]
	[Fast]
	[FromPresence(2)]
	static public async Task Act(TargetSpaceCtx ctx){

		// 2 fear
		ctx.AddFear(2);

		// Push up to 2 explorer/towns
		int pushCount = 2;

		// if target has blight
		if(ctx.HasBlight) {
			// +2 fear
			ctx.AddFear(2);

			// push up to 2 more explorers/towns
			pushCount += 2;
		}

		await ctx.PushUpTo(pushCount,Invader.Explorer,Invader.Town);

		// add 1 blight
		await ctx.AddBlight( 1 );

	}

}