using SpiritIsland;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class StemTheFlowOfFreshWater {
		[SpiritCard( "Stem the Flow of Fresh Water", 0, Speed.Slow, Element.Water, Element.Plant )]
		[FromSacredSite( 1 )]
		static public async Task ActionAsync( TargetSpaceCtx ctx ) {

			// If target land is mountain or sand, 
			if( ctx.IsOneOf( Terrain.Mountain, Terrain.Sand ) ) {
				// instead 1 damange to EACH town/city
				await ctx.PowerInvaders.ApplyDamageToEach(1, Invader.City, Invader.Town);
			} else {
				// 1 damage to 1 town or city.
				var types = ctx.PowerInvaders.Counts.OfAnyType(Invader.City,Invader.Town);
				var invader = await ctx.Self.Action.Choose( new SelectInvaderToDamage( 1, ctx.Target, types, Present.IfMoreThan1 ) );
				if(invader !=null)
					await ctx.PowerInvaders.ApplyDamageTo1(1,invader);
			}

		}

	}

}
