namespace SpiritIsland.JaggedEarth;

public class TheWoundedWildTurnsOnItsAssailants {

	const string Name = "The Wounded Wild Turns on Its Assailants";

	[MajorCard(Name,4,Element.Fire,Element.Plant,Element.Animal), Slow, FromPresence(1,Target.Blight)]
	[Instructions( "Add 2 Badlands. Gather up to 2 Beasts. 1 Damage per Blight / Beasts / Wilds. -If you have- 2 Fire, 3 Plant, 2 Animal: 2 Fear per Invader destroyed by the Power (max. 8 Fear)." ), Artist( Artists.JoshuaWright )]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {
		// Add 2 badlands
		await ctx.Badlands.AddAsync( 2 );

		// Gather up to 2 beast
		await ctx.GatherUpTo(2,Token.Beast);

		// (watch for invaders destroyed in this land)
		int destroyed = 0;
		// the only way a token will be removed, is if it is destroyed
		ctx.Tokens.Adjust(new TokenRemovedHandler(args => destroyed++), 1 );

		// 1 damamge per blight/beast/wilds.
		await ctx.DamageInvaders( ctx.Blight.Count + ctx.Beasts.Count + ctx.Wilds.Count );

		// if you have 2 fire, 3 air, 2 animal
		if( await ctx.YouHave("2 fire,3 air,2 animal"))
			// 2 fear per invader destroyed by this Power (max 8 fear)
			ctx.AddFear( System.Math.Min( 8, destroyed*2));

	}

}