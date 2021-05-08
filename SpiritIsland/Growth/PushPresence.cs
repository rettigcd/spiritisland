﻿using System;
using System.Linq;

namespace SpiritIsland {
	public class PushPresence : GrowthAction {

		public Space From { get; }
		public Space To {get; set;}

		public PushPresence(Spirit spirit, Space from):base(spirit){
			this.From = from; 
			Options = from.SpacesExactly(1)
				.ToArray();
			if(Options.Length == 1)
				To = Options[0];
		}

		public Space[] Options { get; }

		public override void Apply() {
			if(To==null)
				throw new InvalidOperationException("Destination Presence land not specified");
			spirit.Presence.Remove(From);
			spirit.Presence.Add(To);
		}

		public class Resolve : IResolver {
			readonly string from;
			readonly string to;
			public Resolve(string from, string to){ 
				this.from = from;
				this.to = to;
			}
			public void Apply( GrowthOption growthOption ) {
				var action = growthOption.GrowthActions
					.OfType<PushPresence>()
					.VerboseSingle(a=>a.From.Label == from);

				action.To = action.Options.First(x=>x.Label==to);

			}
		}

	}




}
