namespace SpiritIsland;

public class MovePresence : GrowthActionFactory, IActionFactory {

	public int Range { get; }

	public MovePresence(int range) {
		this.Range = range;
	}

	public override async Task ActivateAsync( SelfCtx ctx) {
		var src = await ctx.Decision( Select.DeployedPresence.All("Move presence from:", ctx.Self.Presence, Present.Always ) );
		var dstOptions = ctx.GameState.Tokens[src]
			.Range(Range) // this is ok, since it is a Growth action, not a power action
			.Where( ActionScope.Current.TerrainMapper.IsInPlay );
		var dst = await ctx.Decision( Select.ASpace.ForMoving_SpaceToken("Move presence to:", src, dstOptions, Present.Always, ctx.Self.Token));
		await ctx.Self.Token.Move( src, dst );
	}

	public override string Name => $"MovePresence({Range})";
}