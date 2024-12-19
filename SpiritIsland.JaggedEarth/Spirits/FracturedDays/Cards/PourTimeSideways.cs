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
		var moveOptions = self.Presence.Deployed.SelectMany(from=>self.Presence.Lands
			.Select(to=>new Move{ Source=from, Destination=to }
		)).Where(m=>m.Source.Space != m.Destination)
			.OrderBy(m=>m.Source.Space.Label)
			.ThenBy(m=>m.Destination.Label);

		var move = await self.SelectAlways( "Move presence:", moveOptions );
		await move.Source.MoveTo(move.Destination);

		var srcBoards = move.Source.Space.SpaceSpec.Boards;
		if(srcBoards.Intersect(move.Destination.SpaceSpec.Boards).Any()) return;

		// On the board moved from: During the Invader Phase, Resolve Invader and "Each board / Each land..." Actions one fewer time.
		foreach(var board in srcBoards)
			if(0<board.InvaderActionCount) --board.InvaderActionCount;

		// On the board moved to: During the Invader Phase, Resolve Invader and "Each board / Each Land..." Actions one more time.
		foreach(var board in move.Destination.SpaceSpec.Boards)
			++board.InvaderActionCount;

		GameState.Current.AddTimePassesAction( TimePassesAction.Once( gs => {
			foreach(var b in move.Destination.SpaceSpec.Boards.Union( srcBoards ))
				b.InvaderActionCount = 1;
		}));
	}

}