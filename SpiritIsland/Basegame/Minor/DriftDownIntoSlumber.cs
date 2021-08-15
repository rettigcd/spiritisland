using SpiritIsland;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	public class DriftDownIntoSlumber {

		[MinorCard( "Drift Down into Slumber", 0, Speed.Fast, Element.Air, Element.Earth, Element.Plant )]
		[FromPresence( 2 )]
		public static Task ActAsync( ActionEngine eng, Space target ) {
			int defence = target.Terrain.IsIn( Terrain.Jungle, Terrain.Sand )
				? 4 : 1;
			eng.GameState.Defend( target, defence );
			return Task.CompletedTask;
		}
	}


}
