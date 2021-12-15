using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class VeilTheNightsHunt {

		[MinorCard( "Veil the Night's Hunt", 1, Element.Moon, Element.Air, Element.Animal)]
		[Fast]
		[FromPresence( 2, Target.Dahan )]
		static public Task Act( TargetSpaceCtx ctx ) {

			return ctx.SelectActionOption(
				new SpaceAction( $"Each dahan deals 1 damage to a different invader", ctx => ctx.Apply1DamageToDifferentInvaders( ctx.Dahan.Count ) ),
				new SpaceAction( "push up to 3 dahan", ctx => ctx.PushUpToNDahan( 3 ) )
			);

		}

	}

}
