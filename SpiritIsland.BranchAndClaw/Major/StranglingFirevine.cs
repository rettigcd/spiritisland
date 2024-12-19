namespace SpiritIsland.BranchAndClaw;

public class StranglingFirevine {

	public const string Name = "Strangling Firevine";

	[MajorCard( Name, 4, Element.Fire, Element.Plant ), Slow, FromPresence( Filter.Sands, 1 )]
	[Instructions( "Destroy all Explorer. Add 1 Wilds. Add 1 Wilds in the originating Sands. 1 Damage per Wilds in / adjacent to target land. -If you have- 2 Fire, 3 Plant: +1 Damage per Wilds in / adjacent to target land." ), Artist( Artists.NolanNasser )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		// destroy all explorers.
		await ctx.Invaders.DestroyAll(Human.Explorer);

		// Add 1 wilds.
		await ctx.Wilds.AddAsync(1);

		// Add 1 wilds in the originating Sands. 
		Space[] originatingOptions = TargetSpaceAttribute.TargettedSpace!.Sources;

		Space original = await ctx.Self.Select("Select origination space", originatingOptions, Present.AutoSelectSingle)
			?? throw new InvalidOperationException("Could not find required originating Sands with presence.");
		await original.Wilds.AddAsync(1);

		// 1 damage per wilds in / adjacent to target land.
		int wildsDamage = ctx.Space.InOrAdjacentTo.Sum(s=>s.Wilds.Count);

		// if you have 2 fire, 3 plant: // +1 damage per wilds in / adjacent to target land.
		if(await ctx.YouHave("2 fire,3 plant"))
			wildsDamage += wildsDamage;

		await ctx.DamageInvaders( wildsDamage );

	}

}
