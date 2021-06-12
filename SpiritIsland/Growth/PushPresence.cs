using SpiritIsland.PowerCards;
using System;
using System.Collections.Generic;
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

			new RemovePresence(From).Apply(spirit);
			new AddPresence(To).Apply(spirit);
			To = null;

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
					.OfType<PushPresence>()
					.VerboseSingle(a=>a.From.Label == from);

				action.To = action.Options.First(x=>x.Label==to);
				action.Apply();
			}
		}

		public override bool IsResolved => To != null;


	}



}
