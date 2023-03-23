namespace SpiritIsland.Basegame;
public class CleansingFloods { 

	[MajorCard("Cleansing Floods",5, Element.Sun, Element.Water),Slow,FromPresenceIn(1,Terrain.Wetland)]
	[Instructions( "4 Damage. Remove 1 Blight. -If you have- 4 Water: +10 Damage." ), Artist( Artists.NolanNasser )]
	static public async Task ActAsync(TargetSpaceCtx ctx) {

		// 4 damage
		int damage = 4;

		// remove 1 blight
		await ctx.RemoveBlight();

		// if you have 4 water, +10 damage
		if(await ctx .YouHave("4 water"))
			damage += 10;

		await ctx.DamageInvaders(damage);
	}

}