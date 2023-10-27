namespace SpiritIsland.BranchAndClaw;

public class PyroclasticFlow {

	[MajorCard( "Pyroclastic Flow", 3, Element.Fire, Element.Air, Element.Earth ),Fast,FromPresenceIn( Target.Mountain, 1 )]
	[Instructions( "2 Damage. Destroy all Explorer. If target land is Jungle / Wetland, add 1 Blight. -If you have- 2 Fire, 3 Air, 2 Earth: +4 Damage. Add 1 Wilds." ), Artist( Artists.JoshuaWright )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {
		// 2 damage. Destroy all explorers
		await ctx.Invaders.DestroyAll(Human.Explorer);
		int damage = 2;

		// if target land is J/W, add 1 blight
		if(ctx.IsOneOf(Terrain.Jungle,Terrain.Wetland))
			await ctx .AddBlight(1);

		// if you have 2 fire, 3 air, 2 earth: +4 damage. Add 1 wilds
		if(await ctx.YouHave("2 fire,3 air, 2 earth")) {
			await ctx.Wilds.AddAsync(1);
			damage += 4;
		}
		await ctx.DamageInvaders( damage );
	}

}