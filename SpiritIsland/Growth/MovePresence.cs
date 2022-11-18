namespace SpiritIsland;

public class MovePresence : GrowthActionFactory, IActionFactory {

	public int Range { get; }

	public MovePresence(int range) {
		this.Range = range;
	}

	public override async Task ActivateAsync( SelfCtx ctx) {
		var src = await ctx.Decision( Select.DeployedPresence.All("Move presence from:", ctx.Self,Present.Always ) );
		var dstOptions = ctx.GameState.Tokens[src].Range(Range).Where( ctx.TerrainMapper.IsInPlay );
		var dst = await ctx.Decision( Select.Space.ForAdjacent("Move preseence to:", src, Select.AdjacentDirection.Outgoing, dstOptions.Select(x=>x.Space), Present.Always));
		ctx.Presence.Move( src, dst );
	}

	public override string Name => $"MovePresence({Range})";
}