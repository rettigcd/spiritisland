namespace SpiritIsland.NatureIncarnate;

/// <summary>
/// Allows already-placed Incarna to be moved
/// </summary>
public class MoveOnlyIncarna : SpiritAction {

	public int Range { get; }

	public MoveOnlyIncarna( int range) : base( "Move Incarna - Range "+range ) {
		Range = range;
	}

	public override async Task ActAsync( Spirit self ) {
		// Move Incarna
		if(self.Presence is not IHaveIncarna ihi) return;

		var incarna = ihi.Incarna;
		if(incarna.Space == null) return; // not on board, don't add

		await new TokenMover(self,"Move"
				,incarna.Space
				, incarna.Space.Range(Range).ToArray()
			)
			.AddGroup(1,incarna.Class)
			.DoUpToN();
	}

}