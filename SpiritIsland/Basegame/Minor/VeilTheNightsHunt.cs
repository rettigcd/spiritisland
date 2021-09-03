using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class VeilTheNightsHunt {

		[MinorCard( "Veil the Night's Hunt", 1, Speed.Fast, Element.Moon, Element.Air, Element.Animal)]
		[FromPresence( 2, Target.Dahan )]
		static public Task Act( TargetSpaceCtx ctx ) {

			return ctx.SelectPowerOption(
				new PowerOption( $"Each dahan deals 1 damage to a different invader", ctx => ctx.DamageInvaders( ctx.DahanCount ) ), // !!! Wrong - damage must be on different invaders
				new PowerOption( "push up to 3 dahan", ctx => ctx.PushUpToNTokens( 3, TokenType.Dahan ) )
			);

		}

	}
}
