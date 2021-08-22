using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	[InnatePower( "Ocean Breaks the Shore", Speed.Slow )]
	[FromPresence( 0, Target.Costal )]
	public class OceanBreaksTheShore {

		
		[InnateOption( "2 water,1 earth" )]
		static public Task Option1( TargetSpaceCtx ctx ) {
			// drown 1 town
			return ctx.InvadersOn.Destroy(Invader.Town,1); // !!! Destroy would have nicer syntax if count comes first
		}

		[InnateOption( "3 water,2 earth" )]
		static public Task Option2( TargetSpaceCtx ctx ) {
			// instead drown 1 city
			return ctx.InvadersOn.DestroyAny( 1, Invader.Town, Invader.City );
		}

		[InnateOption( "4 water,3 earth" )]
		static public Task Option3( TargetSpaceCtx ctx ) {
			// also drown 1 town or city
			return ctx.InvadersOn.DestroyAny( 2, Invader.Town, Invader.City );
		}

	}

}
