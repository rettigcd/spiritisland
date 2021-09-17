using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	[InnatePower( "Creepers Tear into Mortar", Speed.Slow )]
	[FromPresence( 0 )]
	class CreepersTearIntoMortar {

		[InnateOption( "1 moon,2 plant", "1 Damage to 1 town/city." )]
		static public Task Option1Async( TargetSpaceCtx ctx ) {
			return DoDamage( ctx, 1 );
		}

		[InnateOption( "2 moon,3 plant","Repeat this Power." )]
		static public Task Option2Async( TargetSpaceCtx ctx ) {
			return DoDamage( ctx, 2 );
		}

		[InnateOption( "3 moon,4 plant","Repeat this Power again." )]
		static public Task Option3Async( TargetSpaceCtx ctx ) {
			return DoDamage(ctx,3);
		}

		static Task DoDamage(TargetSpaceCtx ctx, int damage ) {
			return ctx.Invaders.SmartDamageToTypes( damage, Invader.City, Invader.Town );
		}
	}
}