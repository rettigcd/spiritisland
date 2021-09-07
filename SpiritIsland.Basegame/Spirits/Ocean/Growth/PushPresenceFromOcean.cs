﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class PushPresenceFromOcean : GrowthActionFactory {

		public override async Task ActivateAsync( Spirit self, GameState gameState ) {
			List<Space> pushSpaces = self.Presence.Spaces
				.Where( p => p.Terrain == Terrain.Ocean )
				.Distinct()
				.ToList();

			while(0 < pushSpaces.Count){
				var currentSource = pushSpaces[0];
				var destination = await self.Action.Decide( new TargetSpaceDecision(
					$"Select target of Presence to Push from {currentSource}",
					currentSource.Adjacent
				));

				// apply...
				self.Presence.Move( currentSource, destination );

				// next
				pushSpaces.RemoveAt( 0 );
			}
		}
	}

}
