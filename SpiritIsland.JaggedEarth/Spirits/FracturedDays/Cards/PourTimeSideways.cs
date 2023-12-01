namespace SpiritIsland.JaggedEarth;

class PourTimeSideways {
	const string Name = "Pour Time Sideways";

	[SpiritCard( Name, 1, Element.Moon, Element.Air, Element.Water ), Fast, Yourself]
	[Instructions( "Cost to Use: 3 Time. Move 1 of your Presence to a different land with your Presence. On the board moved from: During the Invader Phase, Resolve Invader and \"Each board / Each land...\" Actions one fewer time. On the board moved to: During the Invader Phase, Resolve Invader and \"Each board / Each Land...\" Actions one more time." ), Artist( Artists.LucasDurham )]
	static public async Task ActAsync( Spirit self ) {
		if(self is not FracturedDaysSplitTheSky frac) return;

		if(!self.Presence.CanMove) return;

		// Cost to Use: 3 Time
		if(frac.Time <3) return;
		await frac.SpendTime( 3 );

		// Move 1 of your presence to a different land with your presence.
		var src = await self.SelectAsync( new A.SpaceToken("Move presence from:", self.Presence.Deployed, Present.Always ) );
		if(!self.Presence.HasMovableTokens( src.Space.Tokens )) return; // !!?? is this necessary?
		var dstOptions = self.Presence.Lands.Tokens().Where( s => s.Space != src.Space );
		var dst = await self.SelectAsync( A.Space.ForMoving_SpaceToken( "Move presence to:", src.Space, dstOptions.Downgrade(), Present.Always, src.Token ) );
		await src.MoveTo(dst);
		var srcBoards = src.Space.Boards;
		if(srcBoards.Intersect(dst.Boards).Any()) return;

		// On the board moved from: During the Invader Phase, Resolve Invader and "Each board / Each land..." Actions one fewer time.
		foreach(var board in srcBoards)
			if(0<board.InvaderActionCount) --board.InvaderActionCount;

		// On the board moved to: During the Invader Phase, Resolve Invader and "Each board / Each Land..." Actions one more time.
		foreach(var board in dst.Boards)
			++board.InvaderActionCount;

		GameState.Current.TimePasses_ThisRound.Push( gs => {
			foreach(var b in dst.Boards.Union( srcBoards ))
				b.InvaderActionCount = 1;
			return Task.CompletedTask;
		} );
	}

}