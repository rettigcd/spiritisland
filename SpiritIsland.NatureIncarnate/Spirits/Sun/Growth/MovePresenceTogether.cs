namespace SpiritIsland.NatureIncarnate;

public class MovePresenceTogether : SpiritAction {

	public MovePresenceTogether():base( "Move up to 3 Presence together" ) { }

	public override async Task ActAsync( SelfCtx ctx ) {

		await new TokenMover(ctx.Self,"Move",
			new SourceSelector(ctx.Self.Presence.Spaces.Tokens()),
			new DestinationSelector( st => st.Space.Range(3).Tokens() )
		)
			.AddGroup(3,ctx.Self.Presence)
			.ConfigSource( SelectFrom.ASingleLand )
			.Config( Distribute.ToASingleLand )
			.DoUpToN();
	}
}
