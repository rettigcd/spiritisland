using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	class QuickenTheEarthsStruggles {

		[MinorCard( "Quicken the Earths Struggles", 1, Speed.Fast, "moon, fire, earth, animal" )]
		[FromSacredSite( 0 )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {

			return ctx.SelectPowerOption(
				new PowerOption( "1 damage to each town/city", () => ctx.PowerInvaders.ApplyDamageToEach( 1, Invader.City, Invader.Town ) ),
				new PowerOption( "defend 10", () => ctx.Defend( 10 ) )
			);

		}


	}
}
