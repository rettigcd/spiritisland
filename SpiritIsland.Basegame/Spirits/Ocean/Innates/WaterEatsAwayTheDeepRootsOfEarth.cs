namespace SpiritIsland.Basegame;

[InnatePower("Water Eats Away the Deep Roots of Earth"), Slow]
[FromPresence(1, Filter.Coastal)]
public class WaterEatsAwayTheDeepRootsOfEarth{


	[InnateTier("2 water", "Add 1 Deeps.",0)]
	static public Task Option1(TargetSpaceCtx ctx) => ctx.Space.AddAsync(Token.Deeps,1);

	[InnateTier("3 water", "Add 1 Deeps to a different Coastal land (on any board).",1)]
	static public Task Option2(TargetSpaceCtx ctx) => AddDeepsToCoastal(1,ctx);

	[InnateTier("3 moon,4 water,3 earth", "Add 1 Deeps to a third Coastal land (on any board).",1)]
	static public Task Option3(TargetSpaceCtx ctx) => AddDeepsToCoastal(2, ctx);

	static async Task AddDeepsToCoastal(int count, TargetSpaceCtx ctx) {
		var used = new HashSet<Space>();
		used.Add(ctx.Space);

		for(int i = 0; i < count; i++) {
			var options = GameState.Current.Spaces.Where(TerrainMapper.Current.IsCoastal).Except(used).ToArray();
			var space = await ctx.Self.Select($"Add additional Deeps ({i + 1} of {count})", options, Present.Done);
			if( space is null ) break;
			await ctx.Space.AddAsync(Token.Deeps, 1);
			used.Add(space);
		}
	}

}
