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
		var src = await ctx.Decision( Select.DeployedPresence.All( "Move presence from:", ctx.Presence, Present.Always ) );
		var dstOptions = ctx.Presence.SpaceStates.Where(s=>s.Space != src);
		var dst = await ctx.Decision( Select.Space.ForAdjacent( "Move preseence to:", src, Select.AdjacentDirection.Outgoing, dstOptions,Present.Always, ctx.Self.Presence.Token ) );
		await ctx.Presence.Move( src, dst );

		if(src.Board == dst.Board) return;

		// On the board moved from: During the Invader Phase, Resolve Invader and "Each board / Each land..." Actions one fewer time.
		foreach(var space in ctx.GameState.Tokens.PowerUp(src.Board.Spaces)) {
			ctx.GameState.Skip1Explore( space, Name );
			ctx.GameState.Skip1Build( space.Space, Name );
			ctx.GameState.Skip1Ravage(space.Space, Name);
		}

		// On the board moved to: During the Invader Phase, Resolve Invader and "Each board / Each Land..." Actions one more time.
		foreach(var space in src.Board.Spaces) {
			ctx.GameState.PourTimeSideways_Add1Build( space );
			ctx.GameState.PourTimeSideways_Add1Explore( space );
			ctx.GameState.PourTimeSideways_Add1Ravage( space );
		}
	}

}