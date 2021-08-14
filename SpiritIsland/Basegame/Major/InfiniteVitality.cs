using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Basegame {
	public class InfiniteVitality {

		[MajorCard( "Infinite Vitality", 3, Speed.Fast, Element.Earth, Element.Plant, Element.Animal )]
		[FromSacredSite( 1 )]
		static public Task ActAsync( ActionEngine eng, Space target ) {
			// dahan have +4 health while in target land.
			// whenever blight would be added to target land, instead leave it on the card

			// if you have 4 earth,
			// dahan ignore damage and destruction effects, 
			// remove 1 blight from target or adjacent land
			return Task.CompletedTask;
		}

	}

}
