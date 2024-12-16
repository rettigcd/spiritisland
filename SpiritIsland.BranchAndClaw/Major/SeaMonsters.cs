namespace SpiritIsland.BranchAndClaw;

public class SeaMonsters {

	[MajorCard( "Sea Monsters", 5, Element.Water, Element.Animal ),Slow,FromPresence( 1, [Filter.Coastal, Filter.Wetland] )]
	[Instructions( "Add 1 Beasts. If Invaders are present, 2 Fear per Beasts (max. 8 Fear). 3 Damage per Beasts. 1 Damage per Blight. -If you have- 3 Water, 3 Animal: Repeat this power." ), Artist( Artists.MoroRogers )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {
		await DoPowerAction( ctx );

		// if you have 3 water and 3 animal: repeat this power
		if(await ctx.YouHave("3 water,3 animal"))
			await DoPowerAction( ctx );
	}

	static async Task DoPowerAction( TargetSpaceCtx ctx ) {
		// add 1 beast.
		var beasts = ctx.Beasts;
		await beasts.AddAsync(1);

		// IF invaders are present,
		if(ctx.HasInvaders)
			// 2 fear per beast (max 8 fear).
			await ctx.AddFear(System.Math.Min(8, beasts.Count * 2));

		int damage = 3 * beasts.Count // 3 damage per beast
			+ ctx.Blight.Count; // 1 damage per blight
		await ctx.DamageInvaders( damage );
	}

}