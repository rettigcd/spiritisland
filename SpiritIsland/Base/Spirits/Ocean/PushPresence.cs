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

		public override void Select( IOption option ) {
			To = (Space)option;
		}

		public override void Apply() {
			if(To==null)
				throw new InvalidOperationException("Destination Presence land not specified");

			new RemovePresence(From).Apply(spirit);
			new AddPresence(To).Apply(spirit);
			To = null;
		}

		public override bool IsResolved => To != null;

	}



}
