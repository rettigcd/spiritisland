using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	public class GatherPresenceIntoOcean : GrowthActionFactory {

		public override IAction Bind( Spirit spirit, GameState gameState ) {
			return new GatherAction(spirit,gameState);
		}

		class GatherAction : BaseAction {
			public GatherAction(Spirit spirit,GameState gs):base(spirit,gs){
				List<Space> oceans = spirit.Presence
					.Where(p=>p.IsCostal)
					.Select(p=>p.Neighbors.Single(o=>o.IsOcean))
					.Distinct()
					.ToList();

				_ = ActAsync(engine);

			}

			static async Task ActAsync(ActionEngine engine){
				List<Space> gatherSpaces = engine.Self.Presence
					.Where(p=>p.IsCostal)
					.Select(p=>p.Neighbors.Single(o=>o.IsOcean))
					.Distinct()
					.ToList();
				
				while(0 < gatherSpaces.Count){

					Space currentTarget = gatherSpaces[0];
					Space source = await engine.SelectSpace(
						$"Select source of Presence to Gather into {currentTarget}"
						,currentTarget.Neighbors
							.Where(engine.Self.Presence.Contains)
							.ToArray()
					);

					// apply...
					engine.Self.Presence.Remove(source);
					engine.Self.Presence.Add(currentTarget);

					// next
					gatherSpaces.RemoveAt(0);

				} // while
			}
		}

	}

}
