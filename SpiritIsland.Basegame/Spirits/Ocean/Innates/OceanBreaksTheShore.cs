using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	[InnatePower( "Ocean Breaks the Shore", Speed.Slow )]
	[FromPresence( 0, Target.Coastal )]
	public class OceanBreaksTheShore {

		
		[InnateOption( "2 water,1 earth" )]
		static public Task Option1( TargetSpaceCtx ctx ) {
			// drown 1 town
			return ctx.Invaders.Destroy(1,Invader.Town);
		}

		[InnateOption( "3 water,2 earth" )]
		static public Task Option2( TargetSpaceCtx ctx ) {
			// instead drown 1 city
			return ctx.Invaders.DestroyAny( 1, Invader.Town, Invader.City );
		}

		[InnateOption( "4 water,3 earth" )]
		static public Task Option3( TargetSpaceCtx ctx ) {
			// also drown 1 town or city
			return ctx.Invaders.DestroyAny( 2, Invader.Town, Invader.City );
		}

	}

}
