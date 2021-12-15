using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	public class DreamOfTheUntouchedLand {

		[MajorCard("Dream of the Untouched Land",6,Element.Moon,Element.Water,Element.Earth,Element.Plant,Element.Animal), Fast, FromSacredSite(1)]
		public static async Task ActAsync(TargetSpaceCtx ctx ) {
			// remove up to 3 blight and up to 3 health worth of invaders
			await ctx.RemoveBlight(3);

			// if you have 3 moon, 2 water  3 earth 2 plant
			if( await ctx.YouHave("3 moon,2 water,3 earth,2 plant" )) {
				// Not implemented!!!

				// Max. (1x/game) add a random new island board next to target board ignore its setup icons.
				// add 2 beast, 2 wilds, 2 badlands and up to 2 presence (from any Spirits) anywhere on it.
				// from now on Build Cards and Each board / Each land" Adversary Actions skip 1 board.
			}



		}

	}


}
