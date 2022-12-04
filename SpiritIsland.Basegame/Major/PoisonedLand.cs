namespace SpiritIsland.Basegame;

public class PoisonedLand {

	[MajorCard("Poisoned Land",3,Element.Earth,Element.Plant,Element.Animal)]
	[Slow]
	[FromPresence(1)]
	static public async Task ActAsync(TargetSpaceCtx ctx){

		// 1 fear
		ctx.AddFear(1);

		// 7 damage
		int damage = 7;

		// add 1 blight
		await ctx.AddBlight(1);

		// destroy all dahan
		await ctx.Dahan.Destroy( ctx.Dahan.Count );

		if(await ctx.YouHave("3 earth,2 plant,2 animal" )) {
			int blightCount = ctx.BlightOnSpace;
			ctx.AddFear(blightCount);
			damage += 4 * blightCount;
		}

		await ctx.DamageInvaders( damage );
	}

}