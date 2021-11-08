using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class PushPresenceFromOcean : GrowthActionFactory {

		public override async Task ActivateAsync( SpiritGameStateCtx ctx ) {

			List<Space> pushSpaces = ctx.Self.Presence.Spaces
				.Where( p => p.Terrain == Terrain.Ocean )
				.Distinct()
				.ToList();

			while(0 < pushSpaces.Count){
				var currentSource = pushSpaces[0];
				string prompt = $"Select target of Presence to Push from {currentSource}";
				// #pushpresence
				var destination = await ctx.Self.Action.Decision( new Decision.Presence.Push( prompt, currentSource, currentSource.Adjacent ));

				// apply...
				ctx.Presence.Move( currentSource, destination );

				// next
				pushSpaces.RemoveAt( 0 );
			}

		}

	}

}
