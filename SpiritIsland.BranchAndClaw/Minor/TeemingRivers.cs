namespace SpiritIsland.BranchAndClaw;

public class TeemingRivers {

	[MinorCard( "Teeming Rivers", 1, Element.Sun, Element.Water, Element.Plant, Element.Animal ),Slow,FromSacredSite( 2, Target.Mountain, Target.Wetland )]
	[Instructions( "If target land has no Blight, add 1 Beasts. If target land has exactly 1 Blight, remove it." ), Artist( Artists.LucasDurham )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		int blightCount = ctx.Blight.Count;

		// if target land has no blight, add 1 beast
		if( blightCount == 0 )
			await ctx.Beasts.Add(1);

		// if target land has exactly 1 blight, remove it
		if( blightCount == 1 )
			await ctx.RemoveBlight();

	}

}