namespace SpiritIsland.Basegame;

[InnatePower( NightTerrors.Name ),Fast]
[FromPresence( 0, Filter.Invaders )]
public class NightTerrors {

	public const string Name = "Night Terrors";

	[InnateTier( "1 moon,1 air","1 fear." )]
	static public Task Option1Async( TargetSpaceCtx ctx ) {
		// 1 fear
		return ctx.AddFear(1);
	}

	[InnateTier( "2 moon,1 air,1 animal", "+1 fear." )]
	static public Task Option2Async( TargetSpaceCtx ctx ) {
		//+1 fear
		return ctx.AddFear( 2 );
	}

	[InnateTier("3 moon,2 air,1 animal", "+1 fear." )]
	static public Task Option3Async( TargetSpaceCtx ctx ) {
		//+1 fear
		return ctx.AddFear( 3 );
	}

}