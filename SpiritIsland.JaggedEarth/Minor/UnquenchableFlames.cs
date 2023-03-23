namespace SpiritIsland.JaggedEarth;
public class UnquenchableFlames{ 
	[MinorCard("Unquenchable Flames",1,Element.Moon,Element.Fire,Element.Earth),Slow,FromSacredSite(2)]
	[Instructions( "1 Fear. 1 Damage to Town / City. Invaders do not heal Damage at end of turn. -If you have- 2 Fire: Add 1 Badlands." ), Artist( Artists.KatGuevara )]
	static public async Task ActAsync( TargetSpaceCtx ctx ){
		// 1 fear.
		ctx.AddFear( 1 );

		// 1 Damage to town/city.
		await ctx.DamageInvaders(1,Human.Town_City);

		// Invaders do not heal Damage at end of turn.
		ctx.GameState.Healer.Skip( ctx.Space );

		// If you have 2 fire: add 1 badlands
		if(await ctx.YouHave("2 fire"))
			await ctx.Badlands.Add(1);
	}

}