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
		Space destination1 = await ctx.MoveTokensToSingleLand(1, new TargetCriteria( 1 ), Token.Beast );
		if(destination1 == null) return;
			
		// As it moves, up to 2 dahan may move with it, for part or all of the way.
		// the beast / dahan may move to an adjacent land and then back.
		var destCtx = ctx.Target(destination1);
		await TokenMover.SingleDestination(destCtx, ctx.Space)
			.AddGroup(2,Human.Dahan)
			.DoUpToN();

		// move beast (2 of 2)
		A.SpaceDecision selection = A.SpaceDecision.ForMoving( $"Move {tracker.BeastMoved.Text} to", destination1.SpaceSpec, destination1.Adjacent, Present.Done, tracker.BeastMoved );
		Space destination2 = await ctx.Self.SelectAsync( selection );
		if(destination2 == null) return;
		await tracker.BeastMoved.MoveAsync(destCtx.Space, destination2);

		var destCtx2 = ctx.Target( destination2 );
		await TokenMover.SingleDestination(destCtx2, destCtx.Space)
			.AddGroup( 2, Human.Dahan )
			.DoUpToN();
	}

	class TrackBeastTokenMoved : BaseModEntity, IHandleTokenRemoved {
		public void HandleTokenRemoved( Space from, ITokenRemovedArgs args ) {
			if(args.Removed.Class == Token.Beast && args is TokenMovedArgs ){
				BeastMoved = args.Removed;
				from.Init(this,0);
			}
		}
		public IToken BeastMoved { get; private set; }
	}

}
