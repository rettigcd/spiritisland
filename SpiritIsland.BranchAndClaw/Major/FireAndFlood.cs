namespace SpiritIsland.BranchAndClaw;

public class FireAndFlood {

	[MajorCard( "Fire and Flood", 7, Element.Sun, Element.Fire, Element.Water )]
	[Slow]
	[FromSacredSite( 1 )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		// == Pick 2nd target - range 2 from same SS ==
		var possibleSacredSiteSourcesForThisSpace = ctx.Tokens.Range(1).Where(s=>ctx.Presence.IsSacredSite(s.Space)).ToArray();

		IEnumerable<Space> secondTargetOptions = ctx.Self.RangeCalc.GetTargetOptionsFromKnownSource(
			ctx,
			TargettingFrom.PowerCard,
			possibleSacredSiteSourcesForThisSpace,
			new TargetCriteria(2)
		);

		var secondTarget = await ctx.Decision( new Select.Space( "Select space to target.", secondTargetOptions, Present.Always ) );


		// 4 damage in each target land  (range must be measured from same SS)
		await ctx.DamageInvaders( 4 );
		await ctx.Target(secondTarget).DamageInvaders(4);
			
		// if 3 fire
		if(await ctx.YouHave("3 fire"))
			await Apply3DamageInOneOfThese( ctx, secondTarget, "fire" );

		// if 3 water
		if(await ctx.YouHave("3 water"))
			await Apply3DamageInOneOfThese( ctx, secondTarget, "water" );

	}

	static async Task Apply3DamageInOneOfThese( TargetSpaceCtx ctx, Space secondTarget, string damageType ) {
		var space = await ctx.Decision( new Select.Space( "Apply 3 " + damageType + " damage to", new Space[] { ctx.Space, secondTarget }, Present.Always ) );
		await ctx.Target( space ).DamageInvaders( 3 );
	}

}