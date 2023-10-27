namespace SpiritIsland.BranchAndClaw;

public class SmotheringInfestation {

	[MajorCard( "Smothering Infestation", 3, Element.Water, Element.Plant),Slow,FromPresence( 0 )]
	[Instructions( "Add 1 Disease. If target land is Jungle / Wetland, 2 Fear and 3 Damage. -If you have- 2 Water, 2 Plant: 1 Damage to each Invader." ), Artist( Artists.JoshuaWright )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {
		// add 1 disease
		await ctx.Disease.AddAsync(1);

		// if you have 2 water and 2 plant:  1 dmaage to each invader (doing this first because our smart-damage result will be better)
		if(await ctx.YouHave("2 water,2 plant"))
			await ctx.DamageEachInvader( 1 );

		// if target land is J/W, 2 fear and 3 damage
		if(ctx.IsOneOf( Terrain.Jungle, Terrain.Wetland )) {
			ctx.AddFear(3);
			await ctx.DamageInvaders(3);
		}

	}

}