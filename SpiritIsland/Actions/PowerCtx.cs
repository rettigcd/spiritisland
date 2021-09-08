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

		static public IEnumerable<Space> PowerAdjacents(Space source) => source
			.Adjacent
			.Where( x => SpaceFilter.ForPowers.TerrainMapper(x) != Terrain.Ocean );

	}

}