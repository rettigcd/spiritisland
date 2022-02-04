namespace SpiritIsland.JaggedEarth;

public class DireMetamorphosis{ 
	[MinorCard("Dire Metamorphosis",1,Element.Moon,Element.Air,Element.Earth,Element.Animal),Slow,FromPresence(1)]
	static public async Task ActAsync(TargetSpaceCtx ctx){
		// 1 fear.
		ctx.AddFear(1);

		// 1 damage.
		await ctx.DamageInvaders(1);

		// 1 Damage to Dahan
		await ctx.DamageDahan(1);

		// Add 1 badlands, 1 beast, 1 disease, 1 strife, 1 wilds, and 1 blight.
		await ctx.Badlands.Add(1);
		await ctx.Beasts.Add(1);
		await ctx.AddStrife();
		await ctx.Wilds.Add(1);
		await ctx.AddBlight(1);
	}

}