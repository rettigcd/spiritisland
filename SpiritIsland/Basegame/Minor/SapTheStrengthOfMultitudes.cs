using SpiritIsland;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	class SapTheStrengthOfMultitudes {

		// !!! need a special targetting method
		// if you have 1 air, increate this power's range by 1


		[MinorCard( "Sap the Strength of Multitudes", 0, Speed.Fast, "water, animal" )]
		[FromPresence( 0 )]
		static public Task ActAsync( ActionEngine engine, Space target ) {
			// defend 5
			engine.GameState.Defend(target,5);
			return Task.CompletedTask;
		}


	}
}
