using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public class PowerCtx : IMakeGamestateDecisions {

		public Spirit Self { get; }

		public GameState GameState { get; }

		public PowerCtx( Spirit self, GameState gameState ) {
			Self = self;
			GameState = gameState;
		}

		// !!! Spirit Powers, Special Rules, & Blight should use this.
		// Oceans are treated as Coastal Wetlands for
		//		* Spirit Powers
		//		* Special Rules
		//		* Blight
		static public IEnumerable<Space> PowerAdjacents(Space source) => source
			.Adjacent
			.Where( x => SpaceFilter.ForPowers.TerrainMapper(x) != Terrain.Ocean );

	}

}