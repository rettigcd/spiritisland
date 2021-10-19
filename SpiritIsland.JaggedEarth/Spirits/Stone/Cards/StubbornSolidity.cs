using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	public class StubbornSolidity {

		[SpiritCard("Stubborn Solidity",1,Element.Sun, Element.Earth, Element.Animal), Fast, FromPresence(1)]
		static public async Task ActAsync(TargetSpaceCtx ctx ) {
			// Defend 1 per dahan  (protects the land)
			ctx.Defend( ctx.DahanCount );

			// dahan in target land cannot be changed. (when they would be damaged, destoryed, removed, replaced, or moved, instead don't)
			// !!! override the Pusher/Gather factory to not move dahan
			// !!! find powers / Fear effects / Events that change the dahan and override it
		}

	}

}
