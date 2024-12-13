namespace SpiritIsland.Basegame;

[InnatePower( PoundShipsToSplinters.Name ),Fast]
[FromPresence( 0, Filter.Coastal )]
public class PoundShipsToSplinters {

	public const string Name = "Pound Ships to Splinters";

	[InnateTier( "1 moon,1 air,2 water","1 fear." )]
	static public Task Option1( TargetSpaceCtx ctx ) {
		// 1 fear
		return ctx.AddFear( 1 );
	}

	[InnateTier( "2 moon,1 air,3 water","+1 fear." )]
	static public async Task Option2( TargetSpaceCtx ctx ) {
		//+1 fear
		await ctx.AddFear( 1 );
		await Option1( ctx );
	}

	[InnateTier( "3 moon,2 air,4 water", "+2 fear." )]
	static public async Task Option3( TargetSpaceCtx ctx ) {
		//+2 fear
		await ctx.AddFear( 2 );
		await Option2( ctx );
	}

}