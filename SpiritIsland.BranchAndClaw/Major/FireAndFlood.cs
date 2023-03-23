namespace SpiritIsland.BranchAndClaw;

public class FireAndFlood {

	[MajorCard( "Fire and Flood", 7, Element.Sun, Element.Fire, Element.Water ),Slow,FromSacredSite( 1 )]
	[Instructions( "4 Damage in each target land. (Range must be measured from the same SacredSite.) -If you have- 3 Fire: +4 Damage in either target land. 3 Water: +4 Damage in either target land." ), Artist( Artists.JasonBehnke )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		Space secondTarget = await PickSecondTarget( ctx );

		// 4 damage in each target land  (range must be measured from same SS)
		await ctx.DamageInvaders( 4 );
		await ctx.Target( secondTarget ).DamageInvaders( 4 );

		// if 3 fire
		if(await ctx.YouHave( "3 fire" ))
			await Apply3DamageInOneOfThese( ctx, secondTarget, "fire" );

		// if 3 water
		if(await ctx.YouHave( "3 water" ))
			await Apply3DamageInOneOfThese( ctx, secondTarget, "water" );

	}

	static async Task<Space> PickSecondTarget( TargetSpaceCtx ctx ) {
		// SS Range 1 from Target
		var possibleSacredSiteSourcesForThisSpace = ctx.Range( 1 )
			.Where( ctx.Self.Presence.IsSacredSite )
			.ToArray();

		// 2nd target options, range 2 from Possible SSs
		return await ctx.Decision( new Select.ASpace( "Select space to target.", ctx.Range( 2 ).Downgrade(), Present.Always ) );
	}

	static async Task Apply3DamageInOneOfThese( TargetSpaceCtx ctx, Space secondTarget, string damageType ) {
		var space = await ctx.Decision( new Select.ASpace( "Apply 3 " + damageType + " damage to", new Space[] { ctx.Space, secondTarget }, Present.Always ) );
		await ctx.Target( space ).DamageInvaders( 3 );
	}

}