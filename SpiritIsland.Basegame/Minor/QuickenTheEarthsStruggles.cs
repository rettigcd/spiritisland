using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	class QuickenTheEarthsStruggles {

		[MinorCard( "Quicken the Earths Struggles", 1, Speed.Fast, "moon, fire, earth, animal" )]
		[FromSacredSite( 0 )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {

			return ctx.SelectActionOption(
				new ActionOption( "1 damage to each town/city", () => ctx.Invaders.ApplyDamageToEach( 1, Invader.City, Invader.Town ) ),
				new ActionOption( "defend 10", () => ctx.Defend( 10 ) )
			);

		}


	}
}
