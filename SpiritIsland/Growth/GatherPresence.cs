using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public class GatherPresence : GrowthAction {

		public Space From { get; set; }
		public Space To {get;}

		public GatherPresence(Spirit spirit, Space target):base(spirit){
			this.To = target; 
			Options = target.SpacesExactly(1)
				.Where(spirit.Presence.Contains)
				.ToArray();
			if(Options.Length == 1)
				From = Options[0];
		}

		public Space[] Options { get; }

		public override void Apply() {
			if(From==null)
				throw new InvalidOperationException("Source Prsence land not specified");
			new RemovePresence(From).Apply(spirit);
			new AddPresence(To).Apply(spirit);
			From = null;
		}

		public class Resolve : IResolver {
			readonly string from;
			readonly string to;
			public Resolve(string from, string to){ 
				this.from = from;
				this.to = to;
			}
			public void Apply(List<GrowthAction> growthActions ) {
				var action = growthActions
					.OfType<GatherPresence>()
					.VerboseSingle(a=>a.To.Label == to);
				action.From = action.Options.First(x=>x.Label==from);
				action.Apply();
			}
		}

		public override bool IsResolved => From != null;


	}

}
