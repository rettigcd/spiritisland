namespace SpiritIsland.FeatherAndFlame;

public class FlashFires {

	[SpiritCard("Flash-Fires",2,Element.Fire,Element.Air),SlowButFastIf("2 air"),FromPresence(1)]
	[Instructions( "1 Fear. 1 Damage. -If you have- 2 Air: This Power is Fast." ), Artist( Artists.NolanNasser )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		// 1 fear
		ctx.AddFear(1);

		// 1 damage
		await ctx.DamageInvaders(1);

		// if you have 2 air, this power is fast

	}

}