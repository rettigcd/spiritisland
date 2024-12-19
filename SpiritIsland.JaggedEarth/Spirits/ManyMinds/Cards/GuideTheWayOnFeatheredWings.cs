namespace SpiritIsland.JaggedEarth;

public class GuideTheWayOnFeatheredWings {

	[SpiritCard("Guide the Way on Feathered Wings", 0, Element.Sun,Element.Air,Element.Animal), Fast, FromPresence(1,Filter.Beast)]
	[Preselect( "Select Guide", "Beast" )]
	[Instructions( "Move 1 Beasts up to two lands. As it moves, up to 2 Dahan may move with it, for part or all of the way. (The Beasts / Dahan may move to an adjacent land and then back.)" ), Artist( Artists.MoroRogers )]
	static public Task ActAsync(TargetSpaceCtx ctx ) {
		// Move 1 beast  up to two lands.
		return MoveBeastAndFriends( ctx );

	}

	static public async Task MoveBeastAndFriends(TargetSpaceCtx ctx ) {

		// track which beast moves
		var tracker = new TrackBeastTokenMoved();
		ctx.Space.Init(tracker,1);

		// move beast (1 of 2)
		Space? destination1 = await ctx.MoveTokensToSingleLand(1, new TargetCriteria( 1 ), Token.Beast );
		if(destination1 is null) return;
			
		// As it moves, up to 2 dahan may move with it, for part or all of the way.
		// the beast / dahan may move to an adjacent land and then back.
		var destCtx = ctx.Target(destination1);
		await TokenMover.SingleDestination(destCtx, ctx.Space)
			.AddGroup(2,Human.Dahan)
			.DoUpToN();

		// move beast (2 of 2)
		var beastMoved = tracker.BeastMoved!; // !!! I'm not sure this is correct, just doing it to make null warnings go away
		A.SpaceDecision selection = A.SpaceDecision.ForMoving( $"Move {beastMoved.Text} to", destination1, destination1.Adjacent, Present.Done, beastMoved);
		Space? destination2 = await ctx.Self.Select( selection );
		if(destination2 is null) return;
		await beastMoved.MoveAsync(destCtx.Space, destination2);

		var destCtx2 = ctx.Target( destination2 );
		await TokenMover.SingleDestination(destCtx2, destCtx.Space)
			.AddGroup( 2, Human.Dahan )
			.DoUpToN();
	}

	class TrackBeastTokenMoved : BaseModEntity, IHandleTokenRemoved {
		public Task HandleTokenRemovedAsync( ITokenRemovedArgs args ) {
			if(args.Removed.Class == Token.Beast && args is TokenMovedArgs ){
				BeastMoved = args.Removed;
				Space from = (Space)args.From;
				from.Init(this,0);
			}
			return Task.CompletedTask;
		}
		public IToken? BeastMoved { get; private set; }
	}

}
