namespace SpiritIsland.Basegame;

public class LandOfHauntsAndEmbers {

	[MinorCard("Land of Haunts and Embers",0,Element.Moon,Element.Fire,Element.Air),Fast,FromPresence(2)]
	[Instructions( "2 Fear. Push up to 2 Explorer / Town. If target land has Blight, +2 Fear. Push up to 2 more Explorer / Town. Add 1 Blight." ), Artist( Artists.JorgeRamos )]
	static public async Task ActAsync(TargetSpaceCtx ctx){

		// 2 fear
		await ctx.AddFear(2);

		// Push up to 2 explorer/towns
		int pushCount = 2;

		// if target has blight
		if(ctx.HasBlight) {
			// +2 fear
			await ctx.AddFear(2);

			// push up to 2 more explorers/towns
			pushCount += 2;
		}

		await ctx.PushUpTo(pushCount,Human.Explorer_Town);

		// add 1 blight
		await ctx.AddBlight( 1 );

	}

}