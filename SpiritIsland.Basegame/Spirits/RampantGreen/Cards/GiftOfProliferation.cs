using SpiritIsland;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class GiftOfProliferation {

		[SpiritCard( "Gift of Proliferation", 1, Element.Moon, Element.Plant ),Fast,AnotherSpirit]
		static public Task ActionAsync( TargetSpiritCtx ctx ) {
			// target spirit adds 1 presense up to range 1 from their presesnse
			return ctx.OtherCtx.PlacePresence(1,Target.Any);
		}

	}
}
