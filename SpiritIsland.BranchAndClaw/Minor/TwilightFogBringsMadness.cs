using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class TwilightFogBringsMadness {

		[MinorCard( "Twilight Fog Brings Madness", 0, Element.Sun, Element.Moon, Element.Air, Element.Water ), Slow, FromPresence( 1 )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			// Add 1 strife
			await ctx.AddStrife();

			// Push 1 dahan
			await ctx.PushDahan( 1 );

			// Each remaining Dahan take 1 damage
			await ctx.Apply1DamageToAllDahan();

		}

	}

}
