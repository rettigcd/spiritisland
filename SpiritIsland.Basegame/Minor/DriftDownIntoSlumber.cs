using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	public class DriftDownIntoSlumber {

		[MinorCard( "Drift Down into Slumber", 0, Speed.Fast, Element.Air, Element.Earth, Element.Plant )]
		[FromPresence( 2 )]
		public static Task ActAsync( TargetSpaceCtx ctx ) {
			// defend 1
			ctx.Defend( 1 );

			// if target land is J/S, instead defend 4
			if( ctx.IsOneOf( Terrain.Jungle, Terrain.Sand ) )
				ctx.Defend( 4-1 ); // -1 is from defend already done above

			return Task.CompletedTask;
		}
	}


}
