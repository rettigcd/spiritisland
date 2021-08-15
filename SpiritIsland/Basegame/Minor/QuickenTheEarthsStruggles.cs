using SpiritIsland.Core;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	class QuickenTheEarthsStruggles {

		[MinorCard( "Quicken the Earths Struggles", 1, Speed.Fast, "moon, fire, earth, animal" )]
		[FromSacredSite( 0 )]
		static public async Task ActAsync( ActionEngine eng, Space target ) {
			if(await eng.SelectTextIndex("Select power option", "1 damage to each town/city","defend 10") == 0) {
				// 1 damage to each town/city
				var grp = eng.GameState.InvadersOn(target);
				foreach(var invader in new Invader[] { Invader.Town1, Invader.Town, Invader.City1, Invader.City2, Invader.City })
					while(grp[invader] > 0)
						grp.ApplyDamageTo1( invader, 1 );
			} else {
				// defend 10
				eng.GameState.Defend(target,10);
			}
		}


	}
}
