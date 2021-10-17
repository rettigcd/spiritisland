using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	[InnatePower( "Creepers Tear into Mortar" ),Slow]
	[FromPresence( 0 )]
	class CreepersTearIntoMortar {

		[InnateOption( "1 moon,2 plant", "1 Damage to 1 town/city." )]
		static public Task Option1Async( TargetSpaceCtx ctx ) {
			return ctx.DamageInvaders(1,Invader.City, Invader.Town);
		}

		// !!! replace repeat with the Attribute so user can target different lands
		[InnateOption( "2 moon,3 plant","Repeat this Power." )]
		static public Task Option2Async( TargetSpaceCtx ctx ) {
			return ctx.DamageInvaders(2,Invader.City, Invader.Town);
		}

		// !!! replace repeat with the Attribute so user can target different lands
		[InnateOption( "3 moon,4 plant","Repeat this Power again." )]
		static public Task Option3Async( TargetSpaceCtx ctx ) {
			return ctx.DamageInvaders(3,Invader.City, Invader.Town);
		}

	}

}