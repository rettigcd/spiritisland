using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class OvergrowInANight {

		[SpiritCard( "Overgrow in a Night", 2, Speed.Fast, Element.Moon, Element.Plant )]
		[FromPresence( 1 )]
		static public Task ActionAsync( TargetSpaceCtx ctx ) {

			return ctx.SelectActionOption(
				new ActionOption("Add 1 presence", () => ctx.Presence_SelectFromTo( ctx.Target )),
				new ActionOption(
					"3 fear",
					() => ctx.AddFear(3), 
					ctx.Self.Presence.IsOn( ctx.Target ) && ctx.Tokens.HasInvaders()  // if presence and invaders
				)
			);

		}

	}
}
