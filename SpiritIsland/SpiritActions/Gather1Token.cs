namespace SpiritIsland;

/// <summary>
/// Gathers a token into 1 of your lands.
/// </summary>
public class Gather1Token : SpiritAction {

	readonly int _range;
	readonly ITokenClass _tokenToGather;

	public Gather1Token( int range, ITokenClass tokenToGather, Present present = Present.Done ):base( "Gather1Token") {
		_range = range;
		_tokenToGather = tokenToGather;
		_present = present;
	}

	public override async Task ActAsync( Spirit self ) {
		// !! can we simplify this?
		var options = self.Presence.Lands.Tokens()
			.SelectMany( p => p.Range( _range ) ) // Growth option so this Range ok
			.Distinct()
			.ToHashSet();

		// !!! simplify this using SourceSelector
		var isInRange = new TargetSpaceCtxFilter( "is in range", x => options.Contains( x.Tokens ) );
		await new SpaceAction( "Gather a " + _tokenToGather.Label, ctx => ctx.Gatherer.AddGroup( 1, _tokenToGather ).DoN(_present) )
			.From().SpiritPickedLand()
			.Which( isInRange )
			.ActAsync( self );
	}

	readonly Present _present;

}