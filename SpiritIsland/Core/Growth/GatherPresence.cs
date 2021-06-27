using System;
using System.Linq;

namespace SpiritIsland.Core {

	public class GatherPresence : GrowthAction {

		public Space From { get; set; }
		public Space To {get;}

		public GatherPresence(Space target){
			this.To = target; 
		}

		public override IOption[] Options => To.SpacesExactly(1)
			.Where(spirit.Presence.Contains)
			.ToArray();

		public override bool IsResolved => From != null;

		public override void Select( IOption option ) {
			From = (Space)option;
		}

		public override void Apply() {
			if(From==null)
				throw new InvalidOperationException("Source Prsence land not specified");
			new RemovePresence(From).Apply(spirit);
			new AddPresence(To).Apply(spirit);
			From = null;
		}

	}

}
