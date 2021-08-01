using System.Linq;
using SpiritIsland.Core;

namespace SpiritIsland.Basegame {

	public class PlaceInOcean : GrowthActionFactory {

		public override void Activate( ActionEngine engine ) {
			var oceanSpaces = engine.GameState.Island.Boards
				.Select( b=>b.Spaces.Single(s=>s.IsOcean) )
				.ToArray();
			_ = PlacePresence.ActAsync(engine,oceanSpaces);
		}

	}

}
