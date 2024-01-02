namespace SpiritIsland.NatureIncarnate;

public class RumblingEarthquakes {

	public const string Name = "Rumbling Earthquakes";

	[MajorCard(Name,6,"fire,earth"),Slow]
	[FromSacredSite(1)]
	[Instructions( "This Power ignores Health bonuses.  3 Fear. 6 Damage to Town/City only. 6 Damage among adjacent lands, to Town/City only. -If you have- 4 earth: 6 Damage among target/adjacent lands to Town/City only." ), Artist( Artists.EmilyHancock )]
	static public async Task ActAsync(TargetSpaceCtx ctx){

		// This Power ignores Health bonuses. - How the hell do we do this part?  What does this even mean?

		// 3 Fear.
		ctx.AddFear(3);

		// 6 Damage to Town/City only.
		await ctx.DamageInvaders(6, Human.Town_City);

		// 6 Damage among adjacent lands, to Town/City only.
		await new SourceSelector(ctx.Tokens.Adjacent)
			.AddAll(Human.Town_City)
			.DoDamageAsync(ctx.Self,6);

		// -If you have- 4 earth:
		if( await ctx.YouHave("4 earth"))
			// 6 Damage among target/adjacent lands to Town/City only.
			await new SourceSelector(ctx.Tokens.Range(1))
				.AddAll(Human.Town_City)
				.DoDamageAsync(ctx.Self,6);


	}

}