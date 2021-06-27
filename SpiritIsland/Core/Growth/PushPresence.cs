using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.Core {

	public class PushPresence : GrowthAction {

		public Space From { get; }
		public Space To {get; set;}

		public PushPresence(Space from){
			this.From = from; 
			Options = from.SpacesExactly(1)
				.ToArray();
			if(Options.Length == 1)
				To = (Space)Options[0];
		}

		public override IOption[] Options { get; }

		public override void Apply() {
			if(To==null)
				throw new InvalidOperationException("Destination Presence land not specified");

			new RemovePresence(From).Apply(spirit);
			new AddPresence(To).Apply(spirit);
			To = null;
		}

		public class Resolve : IResolver {
			readonly string from;
			readonly string to;
			public Resolve(string from, string to){ 
				this.from = from;
				this.to = to;
			}
			public void Apply(List<IAction> growthActions ) {
				PushPresence action = growthActions
					.OfType<PushPresence>()
					.VerboseSingle(a=>a.From.Label == from);

				action.To = (Space)action.Options.First(x=>x.Text==to);
				action.Apply();
				action.Resolved(action.spirit);
			}
		}

		public override bool IsResolved => To != null;

	}



}
