using System;
using System.Linq;

namespace SpiritIsland {

	public class GatherPresence : GrowthAction {

		public string From { get; set; }
		public Space To {get;}

		public GatherPresence(Spirit spirit, Space target):base(spirit){
			this.To = target; 
			Options = target.SpacesExactly(1)
				.Where(spirit.Presence.Contains)
				.ToArray();
			if(Options.Length == 1)
				From = Options[0].Label;
		}

		public Space[] Options { get; }

		public override void Apply() {
			if(From==null)
				throw new InvalidOperationException("Source Prsence land not specified");
			var fromSpace = spirit.Presence.First(x=>x.Label==From);
			spirit.Presence.Remove(fromSpace);
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
					.OfType<GatherPresence>()
					.Single(a=>a.To.Label == to);
				action.From = from;
			}
		}



	}
}
