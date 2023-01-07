namespace SpiritIsland.JaggedEarth;
public class UnquenchableFlames{ 
	[MinorCard("Unquenchable Flames",1,Element.Moon,Element.Fire,Element.Earth),Slow,FromSacredSite(2)]
	static public async Task ActAsync( TargetSpaceCtx ctx ){
		// 1 fear.
		ctx.AddFear( 1 );

		// 1 Damage to town/city.
		await ctx.DamageInvaders(1,Invader.Town_City);

		// Invaders do not heal Damage at end of turn.
		ctx.GameState.Healer.Skip( ctx.Space );

		// If you have 2 fire: add 1 badlands
		if(await ctx.YouHave("2 fire"))
			await ctx.Badlands.Add(1);
	}

}