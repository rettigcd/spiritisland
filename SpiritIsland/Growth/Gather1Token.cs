namespace SpiritIsland;

public class Gather1Token : GrowthActionFactory {

	int _range;
	IEntityClass _tokenToGather;

	public Gather1Token( int range, IEntityClass tokenToGather ) {
		_range = range;
		_tokenToGather = tokenToGather;
	}

	public override async Task ActivateAsync( SelfCtx ctx ) {
		var options = ctx.Self.Presence.Spaces.Tokens()
			.SelectMany( p => p.Range( _range ) ) // Growth option so this Range ok
			.Distinct()
			.ToHashSet();

		//	var to = await ctx.Decision( new Select.ASpace( "Gather beast to", options.Downgrade(), Present.Always ));
		//	await ctx.Target(to).GatherUpTo(1,Token.Beast);

		var isInRange = new TargetSpaceCtxFilter( "is in range", x => options.Contains( x.Tokens ) );
		await new SpaceCmd( "Gather a " + _tokenToGather.Label, ctx => ctx.Pusher.AddGroup( 1, _tokenToGather ).MoveUpToN() )
			.From().SpiritPickedLand().Which( isInRange ).ByPickingToken( _tokenToGather )
			.Execute( ctx );
	}

}