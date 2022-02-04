namespace SpiritIsland.BranchAndClaw;

public class CyclesOfTimeAndTide {

	[MinorCard( "Cycles of Time and Tide", 1, Element.Sun, Element.Moon, Element.Water )]
	[Fast]
	[FromPresence( 1, Target.Coastal )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		if(ctx.Dahan.Any)
			await ctx.Dahan.Add(1);
		else
			await ctx.RemoveBlight();

	}

}