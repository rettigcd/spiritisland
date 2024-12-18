namespace SpiritIsland.BranchAndClaw;

public class BloodwrackPlague {

	[MajorCard("Bloodwrack Plague",4,Element.Water,Element.Earth,Element.Animal),Fast,FromSacredSite(1)]
	[Instructions( "Add 2 Disease. For each Disease in target land, Defend 1 in target and all adjacent lands. -If you have- 2 Earth, 4 Animal: 2 Fear. For each Disease in target land do 1 Damage in target or an adjacent land." ), Artist( Artists.JorgeRamos )]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {
		// add 2 disease
		var disease = ctx.Disease;
		await disease.AddAsync( 2 );

		// for each disease in target land, defend 1 in target and all adjacent lands
		ctx.Defend( disease.Count );
		foreach( var adjacent in ctx.Adjacent )
			ctx.Target(adjacent).Defend( disease.Count );

		// if you have 2 earthn 4 animal:
		if(await ctx.YouHave("2 earth,4 animal")) { 
			// 2 fear.
			await ctx.AddFear(2);
			// For each disease in target land, do 1 damage in target or adjacent land
			int damage = disease.Count;
			var space = await ctx.Self.SelectAlwaysAsync(new A.SpaceDecision($"Select space to apply {damage} damage", ctx.Range(1), Present.Always ));
	
			await ctx.Target(space).DamageInvaders( damage );
		}
	}

}