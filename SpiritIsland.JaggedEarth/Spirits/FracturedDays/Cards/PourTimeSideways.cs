namespace SpiritIsland.JaggedEarth;

class PourTimeSideways {
	const string Name = "Pour Time Sideways";

	[SpiritCard( Name, 1, Element.Moon, Element.Air, Element.Water ), Fast, Yourself]
	static public async Task ActAsync( SelfCtx ctx ) {
		if(ctx.Self is not FracturedDaysSplitTheSky frac) return;
		// Cost to Use: 3 Time
		if(frac.Time <3) return;
		await frac.SpendTime( 3 );

		// Move 1 of your presence to a different land with your presence.
		var src = await ctx.Decision( Select.DeployedPresence.All( "Move presence from:", ctx.Self.Presence, Present.Always ) );
		if(!ctx.Self.Presence.HasMovableTokens( src.Tokens )) return;
		var dstOptions = ctx.Self.Presence.Spaces.Tokens().Where( s => s.Space != src );
		var dst = await ctx.Decision( Select.ASpace.ForMoving_SpaceToken( "Move preseence to:", src, dstOptions, Present.Always, ctx.Self.Token ) );
		await ctx.Self.Presence.Token.Move( src, dst );
		if(src.Boards.Intersect(dst.Boards).Any()) return;

		// On the board moved from: During the Invader Phase, Resolve Invader and "Each board / Each land..." Actions one fewer time.
		foreach(var board in src.Boards)
			if(0<board.InvaderActionCount) --board.InvaderActionCount;

		// On the board moved to: During the Invader Phase, Resolve Invader and "Each board / Each Land..." Actions one more time.
		foreach(var board in dst.Boards)
			++board.InvaderActionCount;

		ctx.GameState.TimePasses_ThisRound.Push( gs => {
			foreach(var b in dst.Boards.Union(src.Boards))
				b.InvaderActionCount = 1;
			return Task.CompletedTask;
		} );
	}

}