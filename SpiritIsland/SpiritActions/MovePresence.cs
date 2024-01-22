namespace SpiritIsland;

public class MovePresence( int _range ) : SpiritAction( $"MovePresence({_range})" ) {

	public int Range { get; } = _range;

	public override async Task ActAsync( Spirit self ) {
		// From
		var src = await self.SelectAsync( new A.SpaceToken( "Move presence from:", self.Presence.Movable, Present.Always ) );

		// To
		var dstOptions = src.Space.Tokens.Range(Range); // this is ok, since it is a Growth action, not a power action
		var dst = await self.SelectAsync( A.Space.ForMoving_SpaceToken( "Move presence to:", src.Space, dstOptions.Downgrade(), Present.Always, src.Token ) );

		// Move
		await src.MoveTo( dst.Tokens );
	}

}