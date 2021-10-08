using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class WrapInWingsOfSunlight {

		[MajorCard( "Wrap in Wings of Sunlight", 3, Speed.Fast, Element.Sun, Element.Air, Element.Animal )]
		[FromPresence(0)]
		static public async Task ActAsync(TargetSpaceCtx ctx) {

			// if you have 2 sun, 2 air, 2 animal, // First Gather up to 3 dahan
			if(ctx.YouHave( "2 sun,2 air,2 animal" ))
				await ctx.GatherUpToNDahan( 3 );

			// move up to 5 dahan from target land to any land.
			// defend 5 in that land

			// destination
			var destination = await ctx.Self.Action.Decision( new Decision.TargetSpace( "Move dahan to", ctx.AllSpaces.Where( s => s.Terrain != Terrain.Ocean ), Present.Always ) );
			// technically could move to ocean while Ocean on board, but no reason to.

			// move dahan
			int max = Math.Min( ctx.DahanCount, 5 );
			int countToMove = await ctx.Self.SelectNumber( "# of dahan to move", max );
			while(countToMove-->0)
				await ctx.Move( TokenType.Dahan.Default, ctx.Space, destination );

			// defend
			ctx.Target(destination).Defend(5);

		}
	}

}
