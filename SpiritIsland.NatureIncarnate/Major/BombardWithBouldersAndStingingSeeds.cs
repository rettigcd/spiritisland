namespace SpiritIsland.NatureIncarnate;

public class BombardWithBouldersAndStingingSeeds {

	public const string Name = "Bombard with Boulders and Stinging Seeds";

	[MajorCard(Name,2,"air,earth,plant"),Slow]
	[FromPresence(Filter.Mountain+","+Filter.Jungle, 2)]
	[Instructions( "1 Fear. 2 Damage. Add 1 Badlands. -If you have- 2 air,2 earth,3 plant: 1 Fear. 2 Damage. Add 1 Wilds." ), Artist( Artists.KatGuevara )]
	static public async Task ActAsync(TargetSpaceCtx ctx){
		// 1 Fear.
		await ctx.AddFear(1);

		// 2 Damage.
		await ctx.DamageInvaders(2);

		// Add 1 Badland.
		await ctx.Badlands.AddAsync(1);

		// -If you have- 2 air,2 earth,3 plant:
		if(await ctx.YouHave("2 air,2 earth,3 plant" )) {
			// 1 Fear.
			await ctx.AddFear(1);
			// 2 Damage.
			await ctx.DamageInvaders(2);
			// Add 1 Wilds.
			await ctx.Wilds.AddAsync(1);
		}

	}

}
