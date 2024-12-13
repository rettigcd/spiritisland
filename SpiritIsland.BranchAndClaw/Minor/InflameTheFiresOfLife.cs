namespace SpiritIsland.BranchAndClaw;

public class InflameTheFiresOfLife {

	[MinorCard( "Inflame the Fires of Life", 1, Element.Moon, Element.Fire, Element.Plant, Element.Animal ),Slow,FromSacredSite( 1 )]
	[Instructions( "Add 1 Disease. -or- 1 Fear. Add 1 Strife. -If you have- 3 Animal: You may do both." ), Artist( Artists.KatBirmelin )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {
		if( await ctx.YouHave("3 animal" )) {
			await AddDisease.ActAsync( ctx );
			await FearAndStrife.ActAsync( ctx );
		} else 
			await ctx.SelectActionOption( AddDisease, FearAndStrife );
	}

	static SpaceAction AddDisease => new SpaceAction( "add disease ", ctx => ctx.Disease.AddAsync(1) );

	static SpaceAction FearAndStrife => new SpaceAction( "1 fear and 1 strife", FearAndStrife_Imp );

	static async Task FearAndStrife_Imp( TargetSpaceCtx ctx ) {
		await ctx.AddFear( 1 );
		await ctx.AddStrife();
	}


}