using System;
using System.Collections.Generic;
using System.Linq;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	public class PushPresenceFromOcean : GrowthActionFactory {

		public override IAction Bind( Spirit ocean, GameState gameState ) {
			return new PushAction(ocean,gameState);
		}

		class PushAction : BaseAction {
			public PushAction(Spirit spirit,GameState gs):base(gs){
				List<Space> oceans = spirit.Presence
					.Where(p=>p.IsOcean)
					.Distinct()
					.ToList();
				engine.decisions.Push(new PushPresenceFrom(engine,spirit,oceans));
			}
		}

		class PushPresenceFrom : IDecision {

			readonly Spirit spirit;
			readonly List<Space> pushSpaces;
			readonly ActionEngine engine;

			public PushPresenceFrom(ActionEngine engine, Spirit spirit,List<Space> pushSpaces){
				this.engine = engine;
				this.spirit = spirit;
				this.pushSpaces = pushSpaces;
			}
			Space CurrentSource => pushSpaces[0];
			public string Prompt => $"Select target of Presence to Push from {CurrentSource}";

			public IOption[] Options => pushSpaces.Count>0 
				? CurrentSource.Neighbors
					.ToArray()
				: Array.Empty<IOption>();

			public void Select( IOption option ) {
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
