namespace SpiritIsland;

public class MovePresence( int _range ) : SpiritAction( $"MovePresence({_range})" ) {

	public int Range { get; } = _range;

	public override async Task ActAsync( Spirit self ) {

		var move = await self.Select( "Move presence", self.Presence.Movable.BuildMoves(x=>x.Space.Range(Range)), Present.Done );
		if( move is not null)
			await move.Apply();

	}

}