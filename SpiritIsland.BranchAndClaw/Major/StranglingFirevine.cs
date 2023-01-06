namespace SpiritIsland.BranchAndClaw;

public class StranglingFirevine {

	[MajorCard( "Strangling Firevine", 4, Element.Fire, Element.Plant )]
	[Slow]
	[FromPresenceIn( 1, Terrain.Sand )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		// destroy all explorers.
		await ctx.Invaders.DestroyNOfClass(int.MaxValue,Invader.Explorer);

		// Add 1 wilds.
		await ctx.Wilds.Add(1);

		// Add 1 wilds in the originating Sands. 
		// !! won't find original if this was picked using a range-extender - would need to capture that info during the targetting process
		var originatingOptions = ctx.Range(1)
			.Select(x=>x.Space)
			.Where( a=> ctx.Presence.Spaces.Contains(a) && ctx.Target(a).Is(Terrain.Sand) ) // using Smart-terrain in case some spirit rule modifies terrain
			.ToArray();
		var originalCtx = await ctx.SelectSpace("Select origination space", originatingOptions, Present.AutoSelectSingle);
		if(originalCtx == null) throw new InvalidOperationException("Could not find required originating Sands with presence.");
		await originalCtx.Wilds.Add(1);

		// 1 damage per wilds in / adjacent to target land.
		int wildsDamage = ctx.Tokens.InOrAdjacentTo.Sum(s=>s.Wilds.Count);

		// if you have 2 fire, 3 plant: // +1 damage per wilds in / adjacent to target land.
		if(await ctx.YouHave("2 fire,3 plant"))
			wildsDamage += wildsDamage;

		await ctx.DamageInvaders( wildsDamage );

	}

}
