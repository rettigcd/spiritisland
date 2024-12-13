namespace SpiritIsland.Basegame;

public class SavageMawbeasts {

	[MinorCard("Savage Mawbeasts",0,Element.Fire,Element.Animal),Slow,FromSacredSite(1)]
	[Instructions( "If target land is Jungle / Wetland, 1 Fear and 1 Damage. -If you have- 3 Animal: 1 Damage." ), Artist( Artists.CariCorene )]
	static public async Task ActAsync(TargetSpaceCtx ctx){
		int damage = 0;

		// if target is J/W, 1 fear & 1 damage
		if(ctx.IsOneOf( Terrain.Jungle, Terrain.Wetland )) {
			++damage;
			await ctx.AddFear(1);
		}

		// If 3 animals +1 damage
		if( await ctx.YouHave("3 animal") )
			++damage;

		await ctx.DamageInvaders( damage );
	}

}