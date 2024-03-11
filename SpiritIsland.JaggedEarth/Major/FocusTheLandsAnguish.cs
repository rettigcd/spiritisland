namespace SpiritIsland.JaggedEarth;

public class FocusTheLandsAnguish {

	[MajorCard("Focus the Land's Anguish",5,Element.Sun), Slow, FromPresence(1)]
	[Instructions( "If this Power Destroys any Town / City, 5 Fear. Gather up to 5 Blight. 1 Damage per Blight.  -If you have- 3 Sun: +1 Damage per Blight." ), Artist( Artists.MoroRogers )]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {
		// If this power Destroys any town/city, 5 fear.
		static int calcUnits(TargetSpaceCtx ctx) => ctx.Space.SumAny(Human.Town_City);
		int initialCount = calcUnits(ctx);

		// Gather up to 5 blight.
		await ctx.GatherUpTo(5,Token.Blight);

		// 1 damamge per blight.
		await ctx.DamageInvaders( ctx.Blight.Count );

		// if you have 3 sun: +1 damage per blight
		if( await ctx.YouHave("3 sun"))
			await ctx.DamageInvaders( ctx.Blight.Count );

		// If this power Destroys any town/city, 5 fear.
		if(calcUnits(ctx) > initialCount)
			ctx.AddFear(5);

	}

}