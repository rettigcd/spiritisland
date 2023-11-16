namespace SpiritIsland.JaggedEarth;

[InnatePower("Air Moves, Earth Endures"), Fast, FromPresence(1)]
class AirMovesEarthEndures {

	[InnateTier("3 air","Push up to 2 explorer or 1 town.")]
	static public Task Option1(TargetSpaceCtx ctx ) {
		return ctx.SelectActionOption( 
			Cmd.PushUpToNExplorers(2), 
			Cmd.PushUpToNTowns(1)
		);
	}

	[InnateTier("3 earth","Defend 5.",1)]
	static public Task Option2(TargetSpaceCtx ctx ) {
		ctx.Defend( 5 );
		return Task.CompletedTask;
	}

}