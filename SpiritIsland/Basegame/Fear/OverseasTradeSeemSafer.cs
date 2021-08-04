using SpiritIsland.Core;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class OverseasTradeSeemSafer : IFearCard {

		[FearLevel( 1, "Defend 3 in all Coastal lands." )]
		public Task Level1( GameState gs ) {
			DefendCostal( gs, 3 );
			// !!! this method has no unit tests
			return Task.CompletedTask;
		}

		[FearLevel( 2, "Defend 6 in all Coastal lands. Invaders do not Build City in Coastal lands this turn." )]
		Task IFearCard.Level2( GameState gs ){
			DefendCostal( gs, 6 );
			SkipCostalBuild( gs ); // !!! bug, should only prevent cities
			return Task.CompletedTask;
		}

		[FearLevel( 3, "Defend 9 in all Coastal lands. Invaders do not Build in Coastal lands this turn." )]
		Task IFearCard.Level3( GameState gs ){
			DefendCostal( gs, 9 );
			SkipCostalBuild( gs );
			return Task.CompletedTask;
		}

		static void DefendCostal( GameState gs, int defense ) {
			foreach(var space in gs.Island.AllSpaces.Where( s => s.IsCostal ))
				gs.Defend( space, defense );
		}

		static void SkipCostalBuild( GameState gs ) {
			foreach(var space in gs.Island.AllSpaces.Where( s => s.IsCostal ))
				gs.SkipBuild(space);
		}

	}
}
