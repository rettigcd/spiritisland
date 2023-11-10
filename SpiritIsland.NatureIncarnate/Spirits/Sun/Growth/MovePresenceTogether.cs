namespace SpiritIsland.NatureIncarnate;

public class MovePresenceTogether : SpiritAction {

	public MovePresenceTogether():base( "Move up to 3 Presence together" ) { }

	public override async Task ActAsync( SelfCtx ctx ) {
		var token = ctx.Self.Presence.Token;
		var from = await ctx.Self.Gateway.Select(new A.Space(
			"Select presence to move up to 3",
			ctx.Self.Presence.Spaces.Tokens(),
			Present.Done,
			token
		));
		if(from == null) return;

		// Select 1st Token destination (like Push, Arrows!)
		var destinationOptions = from.Range(3);
		int remaining = 3;
		Space destination = await ctx.Self.Gateway.Select( A.Space.ToMoveToken( from, destinationOptions.Tokens(), Present.Always, token, remaining ) );
		if(destination == null) return;

		var spaceToken = new SpaceToken( from, token );
		await spaceToken.MoveTo(destination);
		--remaining;
		
		remaining = Math.Min(remaining,ctx.Self.Presence.CountOn(from));

		while(0 < remaining--) {
			var options = new SpaceToken[]{ spaceToken };
			var nextToken = await ctx.Self.Gateway.Select( A.SpaceToken.ToCollect( $"Move up to ({remaining + 1}) to {destination.Text}", options, Present.Done, destination ) );
			if(nextToken == null) break;

			await nextToken.MoveTo(destination);
		}
	}
}
