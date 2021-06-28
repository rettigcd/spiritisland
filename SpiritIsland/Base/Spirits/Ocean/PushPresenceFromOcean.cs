using System;
using System.Collections.Generic;
using System.Linq;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	public class PushPresenceFromOcean : GrowthAction {

		public override IAction Bind( Spirit ocean, GameState gameState ) {
			return new PushAction(ocean,gameState);
		}

		class PushAction : BaseAction {
			public PushAction(Spirit spirit,GameState gs):base(gs){
				List<Space> oceans = spirit.Presence
					.Where(p=>p.IsOcean)
					.Distinct()
					.ToList();
				engine.decisions.Push(new PushPresenceFrom(spirit,oceans));
			}
		}

		class PushPresenceFrom : IDecision {

			readonly Spirit spirit;
			readonly List<Space> pushSpaces;

			public PushPresenceFrom(Spirit spirit,List<Space> pushSpaces){
				this.spirit = spirit;
				this.pushSpaces = pushSpaces;
			}
			Space CurrentSource => pushSpaces[0];
			public string Prompt => $"Select target of Presence to Push from {CurrentSource}";

			public IOption[] Options => pushSpaces.Count>0 
				? CurrentSource.SpacesExactly(1)
					.ToArray()
				: new IOption[0];

			public void Select( IOption option, ActionEngine engine ) {
				// apply...
				Space target = (Space)option;
				spirit.Presence.Remove(CurrentSource);
				spirit.Presence.Add(target);

				// next
				pushSpaces.RemoveAt(0);

				if(pushSpaces.Count>0)
					engine.decisions.Push(this); // reuse this one
			}

		}
	}
}
