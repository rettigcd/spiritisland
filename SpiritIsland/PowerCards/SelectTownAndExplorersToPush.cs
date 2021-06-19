using System.Linq;

namespace SpiritIsland.PowerCards {
	public class SelectTownAndExplorersToPush : IDecision {

		readonly InvaderGroup invaderGroup;
		readonly Space from;
		readonly int count;

		public SelectTownAndExplorersToPush(InvaderGroup invaderGroup,Space from,int count=1){
			this.invaderGroup = invaderGroup;
			this.from = from;
			this.count = count;
		}

		public IOption[] Options { get {
			return invaderGroup
				.InvaderTypesPresent
				.Where(i=>i.Label != "City")  //
				.ToArray();
		}}

		public void Select( IOption option, ActionEngine engine ) {
			var invaderToPush = (Invader)option;

			// if we need more, push next
			if(count>0)
				engine.decisions.Push(new SelectTownAndExplorersToPush(invaderGroup,from,count-1));

			engine.decisions.Push( new SelectInvaderDestination(
				invaderGroup,
				invaderToPush,
				from
			) );
		}

	}

}
