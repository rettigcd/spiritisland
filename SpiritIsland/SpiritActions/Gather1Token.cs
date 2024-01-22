namespace SpiritIsland;

/// <summary>
/// Gathers a token into 1 of your lands.
/// </summary>
public class Gather1Token( int _range, ITokenClass _tokenToGather, Present _present = Present.Done ) 
	: SpiritAction( "Gather1Token")
{

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
}