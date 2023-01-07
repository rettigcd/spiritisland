namespace SpiritIsland.JaggedEarth;

public class FocusTheLandsAnguish {

	[MajorCard("Focus the Land's Anguish",5,Element.Sun), Slow, FromPresence(1)]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {
		// If this power Destroys any town/city, 5 fear.
		static int calcUnits(TargetSpaceCtx ctx) => ctx.Tokens.SumAny(Invader.Town_City);
		int initialCount = calcUnits(ctx);

		// Gather up to 5 blight.
		await ctx.GatherUpTo(5,TokenType.Blight);

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