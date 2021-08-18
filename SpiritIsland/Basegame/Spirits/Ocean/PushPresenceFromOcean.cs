﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {

	public class PushPresenceFromOcean : GrowthActionFactory {

		public override async Task Activate(ActionEngine engine) {
			List<Space> pushSpaces = engine.Self.Presence.Spaces
				.Where( p => p.IsOcean )
				.Distinct()
				.ToList();

			while(0 < pushSpaces.Count){
				var currentSource = pushSpaces[0];
				var destination = await engine.Self.SelectSpace(
					$"Select target of Presence to Push from {currentSource}",
					currentSource.Adjacent
				);

				// apply...
				engine.Self.Presence.Move(currentSource, destination);

				// next
				pushSpaces.RemoveAt( 0 );
			}
		}
	}

}
