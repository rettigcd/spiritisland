﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {

	public class GatherPresenceIntoOcean : GrowthActionFactory {

		public override async Task ActivateAsync( SpiritGameStateCtx ctx ) {
			List<Space> gatherSpaces = ctx.Self.Presence.Spaces
				.Where( p => p.IsCoastal )
				.Select( p => p.Adjacent.Single( o => o.Terrain == Terrain.Ocean ) )
				.Distinct()
				.ToList();

			while(0 < gatherSpaces.Count){

				Space currentTarget = gatherSpaces[0];
				Space source = await ctx.Self.Action.Decision( new Decision.TargetSpace(
					$"Select source of Presence to Gather into {currentTarget}"
					, currentTarget.Adjacent
						.Where( ctx.Self.Presence.Spaces.Contains )
						.ToArray()
					, Present.Always
				));

				// apply...
				ctx.Self.Presence.Move( source, currentTarget );

				// next
				gatherSpaces.RemoveAt( 0 );

			} // while
		}

	}

}
