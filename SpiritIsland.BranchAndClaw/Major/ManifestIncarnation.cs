namespace SpiritIsland.BranchAndClaw;

public class ManifestIncarnation {

	[MajorCard( "Manifest Incarnation", 6, Element.Sun, Element.Moon, Element.Earth, Element.Animal )]
	[Slow]
	[FromPresence( 0, "Cities" )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		// 6 fear
		ctx.AddFear(6);

		// +1 fear for each town/city and for each of your presence in target land.
		int fearCount = ctx.Tokens.SumAny(Invader.City,Invader.Town)
			+ ctx.Self.Presence.Placed.Count(x=>x==ctx.Space);
		ctx.AddFear(fearCount);

		// Remove 1 city, 1 town and 1 explorer.
		await ctx.RemoveInvader(Invader.City);
		await ctx.RemoveInvader(Invader.Town);
		await ctx.RemoveInvader(Invader.Explorer);

		// if you have 3 sun and 3 moon, invaders do -6 damage on their ravage.
		if(await ctx.YouHave("3 sun,3 moon"))
			ctx.Defend( 6 ); // !! not exactly correct but close

		// Then, Invaders in target land ravage.
		await new RavageAction( ctx.GameState, ctx.Invaders ).Exec();
	}

}