namespace SpiritIsland.Basegame;

public class RainOfBlood {

	[MinorCard("Rain of Blood", 0, "air, water, animal"),Slow,FromSacredSite(1,Filter.Invaders)]
	[Instructions( "2 Fear. If target land has at least 2 Town / City, +1 Fear." ), Artist( Artists.KatBirmelin )]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {
		// 2 fear
		await ctx.AddFear(2);

		// if town has at least 2 towns / cities, +1 fear
		if( 2 <= ctx.Invaders.Space.TownsAndCitiesCount() )
			await ctx.AddFear( 1 );
	}


}