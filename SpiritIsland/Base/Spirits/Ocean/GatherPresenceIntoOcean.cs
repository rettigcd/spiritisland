﻿using System;
using System.Collections.Generic;
using System.Linq;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	public class GatherPresenceIntoOcean : GrowthActionFactory {

		public override IAction Bind( Spirit spirit, GameState gameState ) {
			return new GatherAction(spirit,gameState);
		}

		class GatherAction : BaseAction {
			public GatherAction(Spirit spirit,GameState gs):base(gs){
				List<Space> oceans = spirit.Presence
					.Where(p=>p.IsCostal)
					.Select(p=>p.SpacesExactly(1).Single(o=>o.IsOcean))
					.Distinct()
					.ToList();
				engine.decisions.Push(new GatherPresencesInto(engine,spirit,oceans));
			}
		}

		class GatherPresencesInto : IDecision {
			readonly Spirit spirit;
			readonly List<Space> gatherSpaces;
			readonly ActionEngine engine;
			public GatherPresencesInto(ActionEngine engine, Spirit spirit, List<Space> gatherSpaces){
				this.engine = engine;
				this.spirit = spirit;
				this.gatherSpaces = gatherSpaces;
			}
			Space CurrentTarget => gatherSpaces[0];
			public string Prompt => $"Select source of Presence to Gather into {CurrentTarget}";

			public IOption[] Options => gatherSpaces.Count>0 
				? CurrentTarget.SpacesExactly(1)
					.Where(spirit.Presence.Contains)
					.ToArray()
				: Array.Empty<IOption>();

			public void Select( IOption option) {
				// apply...
				Space source = (Space)option;
				spirit.Presence.Remove(source);
				spirit.Presence.Add(CurrentTarget);

				// next
				gatherSpaces.RemoveAt(0);

				if(gatherSpaces.Count>0)
					engine.decisions.Push(this); // reuse this one
			}
		}

	}

}
