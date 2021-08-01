using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Basegame {

	public class PushPresenceFromOcean : GrowthActionFactory {

		public override void Activate(ActionEngine engine) {
			_ = ActAsync( engine );
		}

		static async Task ActAsync(ActionEngine engine){
			List<Space> pushSpaces = engine.Self.Presence
				.Where(p=>p.IsOcean)
				.Distinct()
				.ToList();

			while(0<pushSpaces.Count){
				var currentSource = pushSpaces[0];
				var destination = await engine.SelectSpace(
					$"Select target of Presence to Push from {currentSource}",
					currentSource.Neighbors
				);

				// apply...
				engine.Self.Presence.Remove(currentSource);
				engine.Self.Presence.Add(destination);

				// next
				pushSpaces.RemoveAt(0);
			}

		}
	}

}
