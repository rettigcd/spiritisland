namespace SpiritIsland.Basegame;

[InnatePower( PoundShipsToSplinters.Name ),Fast]
[FromPresence( 0, Filter.Coastal )]
public class PoundShipsToSplinters {

	public const string Name = "Pound Ships to Splinters";

	[InnateTier( "1 moon,1 air,2 water","1 fear." )]
	static public Task OneFear( TargetSpaceCtx ctx ) {
		// 1 fear
		return ctx.AddFear( 1 );
	}

	[InnateTier( "2 moon,1 air,3 water","+1 fear." )]
	static public async Task TwoFear( TargetSpaceCtx ctx ) {
		//+1 fear
		await ctx.AddFear( 1 );
		await OneFear( ctx );
	}

	[InnateTier( "3 moon,2 air,4 water", "+2 fear." )]
	static public async Task ThreeFear( TargetSpaceCtx ctx ) {
		//+2 fear
		await ctx.AddFear( 2 );
		await TwoFear( ctx );
	}

}