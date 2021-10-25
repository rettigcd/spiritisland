using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class StemTheFlowOfFreshWater {

		[SpiritCard( "Stem the Flow of Fresh Water", 0, Element.Water, Element.Plant )]
		[Slow]
		[FromSacredSite( 1 )]
		static public async Task ActionAsync( TargetSpaceCtx ctx ) {

			// If target land is mountain or sand, 
			if( ctx.IsOneOf( Terrain.Mountain, Terrain.Sand ) ) {
				// instead 1 damange to EACH town/city
				await ctx.DamageEachInvader(1, Invader.City, Invader.Town);
			} else {
				await ctx.DamageInvaders( 1, Invader.Town, Invader.City );
			}

		}

	}

}
