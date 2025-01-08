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
		await TokenMover.SingleDestination(ctx.Target((Space)move1.Destination), ctx.Space)
			.UseQuota(new Quota().AddGroup(2, Human.Dahan))
			.DoUpToN();

		var stop1 = (Space)move1.Destination;
		var move2 = await ctx.Self.Select("Move Beast (2 of 2)", move1.Source.Token.On(stop1).BuildMoves(stop1.Adjacent), Present.Done );
		if(move2 is null) return;
		await move2.Apply();

		// As it moves, up to 2 dahan may move with it, for part or all of the way.
		// the beast / dahan may move to an adjacent land and then back.
		await TokenMover.SingleDestination(ctx.Target((Space)move2.Destination), stop1)
			.UseQuota(new Quota().AddGroup(2, Human.Dahan))
			.DoUpToN();
	}

}
