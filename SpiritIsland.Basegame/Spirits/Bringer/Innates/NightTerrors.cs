namespace SpiritIsland.Basegame;

[InnatePower( NightTerrors.Name ),Fast]
[FromPresence( 0, Filter.Invaders )]
public class NightTerrors {

	public const string Name = "Night Terrors";

	[InnateTier( "1 moon,1 air","1 fear." )]
	static public Task OneFear( TargetSpaceCtx ctx ) {
		// 1 fear
		return ctx.AddFear(1);
	}

	[InnateTier( "2 moon,1 air,1 animal", "+1 fear." )]
	static public Task TwoFear( TargetSpaceCtx ctx ) {
		//+1 fear
		return ctx.AddFear( 2 );
	}

	[InnateTier("3 moon,2 air,1 animal", "+1 fear." )]
	static public Task ThreeFear( TargetSpaceCtx ctx ) {
		//+1 fear
		return ctx.AddFear( 3 );
	}

}