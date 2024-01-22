namespace SpiritIsland.NatureIncarnate;

/// <summary>
/// Allows already-placed Incarna to be moved
/// </summary>
public class MoveOnlyIncarna( int range ) 
	: SpiritAction( "Move Incarna - Range "+range )
{

	public int Range { get; } = range;

	public override async Task ActAsync( Spirit self ) {
		var incarna = self.Incarna;
		if(!incarna.IsPlaced) return; // not on board, don't add

		var space = incarna.Space;
		await new TokenMover(self,"Move",space,space.Range(Range).ToArray())
			.AddGroup( 1, incarna )
			.DoUpToN();
	}

}