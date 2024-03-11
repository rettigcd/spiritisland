using System;

namespace SpiritIsland.Basegame;

public class WrapInWingsOfSunlight {

	[MajorCard( "Wrap in Wings of Sunlight", 3, Element.Sun, Element.Air, Element.Animal ), Fast, FromPresence(0)]
	[Instructions( "Move up to 5 Dahan from target land to any land. Defend 5 in that land. -If you have- 2 Sun, 2 Air, 2 Animal: First, Gather up to 3 Dahan." ), Artist( Artists.LoicBelliau )]
	static public async Task ActAsync(TargetSpaceCtx ctx) {

		// if you have 2 sun, 2 air, 2 animal, // First Gather up to 3 dahan
		if(await ctx.YouHave( "2 sun,2 air,2 animal" ))
			await ctx.GatherUpToNDahan( 3 );

		// move up to 5 dahan from target land to any land.
		Space destination = await ctx.MoveTokensToSingleLand( max:5, new TargetCriteria( 100 ), Human.Dahan );

		// defend 5 in that land
		if( destination != null )
			ctx.TargetSpec( destination.SpaceSpec ).Defend( 5 );

	}

}