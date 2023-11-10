namespace SpiritIsland.NatureIncarnate;

public class MovePresenceTogether : SpiritAction {

	public MovePresenceTogether():base( "Move up to 3 Presence together" ) { }

	public override async Task ActAsync( SelfCtx ctx ) {

		var srcToken = await ctx.Self.Select( A.SpaceToken.OfDeployedPresence("Select presence to move up to 3",ctx.Self,Present.Done) );

		if(srcToken == null) return;

		// Select 1st Token destination (like Push, Arrows!)
		var destinationOptions = srcToken.Space.Range(3);
		int remaining = 3;
		Space destination = await ctx.Self.Select( A.Space.ToMoveToken( srcToken, destinationOptions, Present.Always, remaining ) );
		if(destination == null) return;

		await srcToken.MoveTo(destination);
		--remaining;
		
		remaining = Math.Min(remaining,ctx.Self.Presence.CountOn(srcToken.Space));

		while(0 < remaining--) {
			var options = new SpaceToken[]{ srcToken };
			var nextToken = await ctx.Self.Select( A.SpaceToken.ToCollect( $"Move up to ({remaining + 1}) to {destination.Text}", options, Present.Done, destination ) );
			if(nextToken == null) break;

			await nextToken.MoveTo(destination);
		}
	}
}
