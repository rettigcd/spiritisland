using SpiritIsland.PowerCards;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public class GatherPresence : GrowthAction {

		public Space From { get; set; }
		public Space To {get;}

		public GatherPresence(Space target){
			this.To = target; 
		}

		public override IOption[] Options => To.SpacesExactly(1)
			.Where(spirit.Presence.Contains)
			.ToArray();

		public override void Apply() {
			if(From==null)
				throw new InvalidOperationException("Source Prsence land not specified");
			new RemovePresence(From).Apply(spirit);
			new AddPresence(To).Apply(spirit);
			From = null;
			spirit.MarkResolved( this );
		}

		public class Resolve : IResolver {
			readonly string from;
			readonly string to;
			public Resolve(string from, string to){ 
				this.from = from;
				this.to = to;
			}
			public void Apply(List<IAction> growthActions ) {
				var action = growthActions
					.OfType<GatherPresence>()
					.VerboseSingle(a=>a.To.Label == to);
				action.From = (Space)action.Options.First(x=>x.Text==from);
				action.Apply();
			}
		}

		public override bool IsResolved => From != null;


	}

}
