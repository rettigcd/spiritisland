namespace SpiritIsland.JaggedEarth;

class PourTimeSideways {
	const string Name = "Pour Time Sideways";

	[SpiritCard( Name, 1, Element.Moon, Element.Air, Element.Water ), Fast, Yourself]
	[Instructions( "Cost to Use: 3 Time. Move 1 of your Presence to a different land with your Presence. On the board moved from: During the Invader Phase, Resolve Invader and \"Each board / Each land...\" Actions one fewer time. On the board moved to: During the Invader Phase, Resolve Invader and \"Each board / Each Land...\" Actions one more time." ), Artist( Artists.LucasDurham )]
	static public async Task ActAsync( Spirit self ) {
		if(self is not FracturedDaysSplitTheSky frac) return;

		// Cost to Use: 3 Time
		if(frac.Time <3) return;
		await frac.SpendTime( 3 );

		// Move 1 of your presence to a different land with your presence.
		var src = await self.SelectAlwaysAsync( new A.SpaceTokenDecision("Move presence from:", self.Presence.Deployed, Present.Always ) );
		if(!src.Space.Has(self.Presence)) return; // !!?? is this necessary?
		IEnumerable<Space> dstOptions = self.Presence.Lands.Where( s => s != src.Space );
		Space dst = await self.SelectAlwaysAsync( A.SpaceDecision.ForMoving( "Move presence to:", src.Space.SpaceSpec, dstOptions, Present.Always, src.Token ) );
		await src.MoveTo(dst);
		var srcBoards = src.Space.SpaceSpec.Boards;
		if(srcBoards.Intersect(dst.SpaceSpec.Boards).Any()) return;

		// On the board moved from: During the Invader Phase, Resolve Invader and "Each board / Each land..." Actions one fewer time.
		foreach(var board in srcBoards)
			if(0<board.InvaderActionCount) --board.InvaderActionCount;

		// On the board moved to: During the Invader Phase, Resolve Invader and "Each board / Each Land..." Actions one more time.
		foreach(var board in dst.SpaceSpec.Boards)
			++board.InvaderActionCount;

		GameState.Current.AddTimePassesAction( TimePassesAction.Once( gs => {
			foreach(var b in dst.SpaceSpec.Boards.Union( srcBoards ))
				b.InvaderActionCount = 1;
		}));
	}

}