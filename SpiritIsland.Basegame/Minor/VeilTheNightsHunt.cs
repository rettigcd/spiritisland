using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class VeilTheNightsHunt {

		[MinorCard( "Veil the Night's Hunt", 1, Element.Moon, Element.Air, Element.Animal)]
		[Fast]
		[FromPresence( 2, Target.Dahan )]
		static public Task Act( TargetSpaceCtx ctx ) {

			return ctx.SelectActionOption(
				new ActionOption( $"Each dahan deals 1 damage to a different invader", () => ctx.Apply1DamageToDifferentInvaders( ctx.Dahan.Count ) ),
				new ActionOption( "push up to 3 dahan", () => ctx.PushUpToNDahan( 3 ) )
			);

		}

	}

}
