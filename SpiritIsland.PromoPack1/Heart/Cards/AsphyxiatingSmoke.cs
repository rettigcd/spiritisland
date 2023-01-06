namespace SpiritIsland.PromoPack1;

public class AsphyxiatingSmoke {

	[SpiritCard("Asphyxiating Smoke",2,Element.Fire,Element.Air,Element.Plant)]
	[Slow]
	[FromSacredSite(2)] 
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		// 1 fear
		ctx.AddFear(1);

		// destroy 1 town
		await ctx.Invaders.DestroyNOfClass(1,Invader.Town);

		// push 1 dahan
		await ctx.PushDahan(1);

	}

}