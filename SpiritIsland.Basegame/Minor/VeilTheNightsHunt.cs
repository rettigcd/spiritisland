using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class VeilTheNightsHunt {

		[MinorCard( "Veil the Night's Hunt", 1, Speed.Fast, Element.Moon, Element.Air, Element.Animal)]
		[FromPresence( 2, Target.Dahan )]
		static public Task Act( TargetSpaceCtx ctx ) {

			return ctx.SelectActionOption(
				// !!! Wrong - damage must be on different invaders
				new ActionOption( $"Each dahan deals 1 damage to a different invader", () => ctx.DamageInvaders( ctx.DahanCount ) ),
				new ActionOption( "push up to 3 dahan", () => ctx.PushUpToNTokens( 3, TokenType.Dahan ) )
			);

		}

	}
}
