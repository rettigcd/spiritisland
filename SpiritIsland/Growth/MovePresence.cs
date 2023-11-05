using SpiritIsland.Select;

namespace SpiritIsland;

public class MovePresence : SpiritAction {

	public int Range { get; }

	public MovePresence(int range):base( $"MovePresence({range})" ) {
		this.Range = range;
	}

	public override async Task ActAsync( SelfCtx ctx) {
		// From
		var src = await ctx.Decision( new ASpaceToken( "Move presence from:", ctx.Self.Presence.Movable, Present.Always ) );

		// To
		var dstOptions = src.Space.Tokens.Range(Range); // this is ok, since it is a Growth action, not a power action
		var dst = await ctx.Decision( Select.ASpace.ForMoving_SpaceToken( "Move presence to:", src.Space, dstOptions, Present.Always, src.Token ) );

		// Move
		await src.MoveTo( dst.Tokens );
	}

}