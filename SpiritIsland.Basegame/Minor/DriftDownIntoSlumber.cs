using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	public class DriftDownIntoSlumber {

		[MinorCard( "Drift Down into Slumber", 0, Speed.Fast, Element.Air, Element.Earth, Element.Plant )]
		[FromPresence( 2 )]
		public static Task ActAsync( TargetSpaceCtx ctx ) {
			int defence = ctx.IsOneOf( Terrain.Jungle, Terrain.Sand )
				? 4 : 1;
			ctx.Defend( defence );
			return Task.CompletedTask;
		}
	}


}
