﻿using System;
using System.Collections.Generic;
using System.Linq;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	public class GatherPresenceIntoOcean : GrowthAction {

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
				engine.decisions.Push(new GatherPresencesInto(spirit,oceans));
			}
		}

		class GatherPresencesInto : IDecision {
			Spirit spirit;
			List<Space> gatherSpaces;
			public GatherPresencesInto(Spirit spirit, List<Space> gatherSpaces){
				this.spirit = spirit;
				this.gatherSpaces = gatherSpaces;
			}
			Space currentTarget => gatherSpaces[0];
			public string Prompt => $"Select source of Presence to Gather into {currentTarget}";

			public IOption[] Options => gatherSpaces.Count>0 
				? currentTarget.SpacesExactly(1)
					.Where(spirit.Presence.Contains)
					.ToArray()
				: new IOption[0];

			public void Select( IOption option, ActionEngine engine ) {
				// apply...
				Space source = (Space)option;
				spirit.Presence.Remove(source);
				spirit.Presence.Add(currentTarget);

				// next
				gatherSpaces.RemoveAt(0);

				if(gatherSpaces.Count>0)
					engine.decisions.Push(this); // reuse this one
			}
		}

	}

}