using SpiritIsland.Select;

namespace SpiritIsland;

public class MovePresence : GrowthActionFactory, IActionFactory {

	public int Range { get; }

	public MovePresence(int range) {
		this.Range = range;
	}

	public override async Task ActivateAsync( SelfCtx ctx) {
		// From
		var src = await ctx.Decision( new ASpaceToken( "Move presence from:", ctx.Self.Presence.Movable, Present.Always ) );

		// To
		var dstOptions = src.Space.Tokens.Range(Range); // this is ok, since it is a Growth action, not a power action
		var dst = await ctx.Decision( Select.ASpace.ForMoving_SpaceToken( "Move presence to:", src.Space, dstOptions, Present.Always, src.Token ) );

		// Move
		await src.MoveTo( dst.Tokens );
	}

	public override string Name => $"MovePresence({Range})";
}