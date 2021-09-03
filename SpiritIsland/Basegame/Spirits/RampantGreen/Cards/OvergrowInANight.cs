using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class OvergrowInANight {

		[SpiritCard( "Overgrow in a Night", 2, Speed.Fast, Element.Moon, Element.Plant )]
		[FromPresence( 1 )]
		static public Task ActionAsync( TargetSpaceCtx ctx ) {

			return ctx.SelectPowerOption(
				new PowerOption("Add 1 presence", ctx => ctx.PlacePresence( ctx.Target )),
				new PowerOption(
					"3 fear", 
					ctx => ctx.AddFear(3), 
					ctx.Self.Presence.IsOn( ctx.Target ) && ctx.Tokens.HasInvaders()  // if presence and invaders
				)
			);

		}

	}
}
