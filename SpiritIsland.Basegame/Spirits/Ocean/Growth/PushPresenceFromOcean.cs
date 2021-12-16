using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class PushPresenceFromOcean : GrowthActionFactory {

		public override async Task ActivateAsync( SelfCtx ctx ) {

			List<Space> pushSpaces = ctx.Self.Presence.Spaces
				.Where( p => p.Terrain == Terrain.Ocean )
				.Distinct()
				.ToList();

			while(0 < pushSpaces.Count){
				var currentSource = pushSpaces[0];

				// #pushpresence
				var destination = await ctx.Decision( Select.Space.PushPresence( currentSource, currentSource.Adjacent, Present.Always ));

				// apply...
				ctx.Presence.Move( currentSource, destination );

				// next
				pushSpaces.RemoveAt( 0 );
			}

		}

	}

}
