using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class OvergrowInANight {

		[SpiritCard( "Overgrow in a Night", 2, Speed.Fast, Element.Moon, Element.Plant )]
		[FromPresence( 1 )]
		static public Task ActionAsync( TargetSpaceCtx ctx ) {

			return ctx.SelectActionOption(
				new ActionOption("Add 1 presence", () => ctx.PlacePresence( ctx.Space )),
				new ActionOption(
					"3 fear",
					() => ctx.AddFear(3), 
					ctx.HasSelfPresence && ctx.Tokens.HasInvaders()  // if presence and invaders
				)
			);

		}

	}
}
