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

		var move1 = await ctx.Self.Select("Move Beast (1 of 2)", ctx.Space.OfTag(Token.Beast).On(ctx.Space).BuildMoves(x=>ctx.Space.Adjacent), Present.Done );
		if(move1 is null) return;
		await move1.Apply();

		// As it moves, up to 2 dahan may move with it, for part or all of the way.
		// the beast / dahan may move to an adjacent land and then back.
		await TokenMover.SingleDestination(ctx.Target(move1.Destination), ctx.Space)
			.AddGroup(2, Human.Dahan)
			.DoUpToN();

		var move2 = await ctx.Self.Select("Move Beast (2 of 2)", new SpaceToken(move1.Destination,move1.Source.Token).BuildMoves(move1.Destination.Adjacent), Present.Done );
		if(move2 is null) return;
		await move2.Apply();

		// As it moves, up to 2 dahan may move with it, for part or all of the way.
		// the beast / dahan may move to an adjacent land and then back.
		await TokenMover.SingleDestination(ctx.Target(move2.Destination), move1.Destination)
			.AddGroup(2, Human.Dahan)
			.DoUpToN();
	}

}
