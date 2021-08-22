using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class WrapInWingsOfSunlight {

		[MajorCard( "Wrap in Wings of Sunlight", 3, Speed.Fast, Element.Sun, Element.Air, Element.Animal )]
		[FromPresence(0)]
		static public async Task ActAsync(TargetSpaceCtx ctx) {

			// if you have 2 sun, 2 air, 2 animal, // First Gather up to 3 dahan
			if(ctx.Self.Elements.Contains( "2 sun,2 air,2 animal" ))
				await ctx.GatherUpToNDahan( 3 );

			// move up to 5 dahan from target land to any land.
			// defend 5 in that land

			// destination
			var destination = await ctx.Self.SelectSpace( "Move dahan to", ctx.GameState.Island.AllSpaces.Where( s => s.Terrain != Terrain.Ocean ) );
			// technically could move to ocean while Ocean on board, but no reason to.

			// move dahan
			int max = Math.Min( ctx.DahanCount, 5 );
			int countToMove = await ctx.Self.SelectNumber( "# of dahan to move", max );
			await ctx.GameState.MoveDahan( ctx.Target, destination, countToMove );

			// defend
			ctx.GameState.Defend( destination, 5 );

		}
	}

}
