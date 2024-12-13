namespace SpiritIsland.FeatherAndFlame;

public class AsphyxiatingSmoke {

	[SpiritCard("Asphyxiating Smoke",2,Element.Fire,Element.Air,Element.Plant),Slow,FromSacredSite(2)]
	[Instructions( "1 Fear. Destroy 1 Town. Push 1 Dahan." ), Artist( Artists.NolanNasser )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		// 1 fear
		await ctx.AddFear(1);

		// destroy 1 town
		await ctx.Invaders.DestroyNOfClass(1,Human.Town);

		// push 1 dahan
		await ctx.PushDahan(1);

	}

}