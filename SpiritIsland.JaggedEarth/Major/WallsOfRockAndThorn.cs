namespace SpiritIsland.JaggedEarth;
public class WallsOfRockAndThorn {

	[MajorCard("Walls of Rock and Thorn",4,Element.Sun,Element.Earth,Element.Plant), Fast, FromSacredSite(2,Target.Jungle, Target.Mountain )]
	[Instructions( "2 Damage. Defend 8. Add 1 Wilds. Isolate target land. -If you have- 2 Earth, 2 Plant: +2 Damage. +2 Defend. Add 1 Badlands." ), Artist( Artists.JoshuaWright )]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {
		// 2 damage.
		int damage = 2;
		// defend 8.
		int defend = 8;

		bool hasBonus = await ctx.YouHave("2 earth,2 plant" );
		if(hasBonus) {
			// +2 damage.
			damage += 2;
			// +2 defend.
			defend += 2;
		}

		await ctx.DamageInvaders( damage );
		ctx.Defend(defend);

		// add 1 wilds.
		await ctx.Wilds.AddAsync(1);

		// isolate target land
		ctx.Isolate();

		// if you have 2 earth, 2 plant:
		if( hasBonus )
			// Add 1 badland.
			await ctx.Badlands.AddAsync(1);

	}

}