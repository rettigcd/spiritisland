using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	class QuickenTheEarthsStruggles {

		[MinorCard( "Quicken the Earths Struggles", 1, Speed.Fast, "moon, fire, earth, animal" )]
		[FromSacredSite( 0 )]
		static public async Task ActAsync( ActionEngine eng, Space target ) {
			if(await eng.SelectFirstText("Select power option", "1 damage to each town/city","defend 10"))
				// 1 damage to each town/city
				eng.GameState.InvadersOn(target).ApplyDamageToEach(1,Invader.City,Invader.Town);
			else
				// defend 10
				eng.GameState.Defend(target,10);
		}


	}
}
