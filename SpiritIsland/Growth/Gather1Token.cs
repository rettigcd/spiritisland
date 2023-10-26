namespace SpiritIsland;

public class Gather1Token : GrowthActionFactory {

	readonly int _range;
	readonly IEntityClass _tokenToGather;

	public Gather1Token( int range, IEntityClass tokenToGather ) {
		_range = range;
		_tokenToGather = tokenToGather;
	}

	public override async Task ActivateAsync( SelfCtx ctx ) {
		var options = ctx.Self.Presence.Spaces.Tokens()
			.SelectMany( p => p.Range( _range ) ) // Growth option so this Range ok
			.Distinct()
			.ToHashSet();

		var isInRange = new TargetSpaceCtxFilter( "is in range", x => options.Contains( x.Tokens ) );
		await new SpaceCmd( "Gather a " + _tokenToGather.Label, ctx => ctx.Gatherer.AddGroup( 1, _tokenToGather ).GatherUpToN() )
			.From().SpiritPickedLand().Which( isInRange )
			.Execute( ctx );
	}

}