using System.Linq;

namespace SpiritIsland.PowerCards {

	public class SelectInvadersToPush : IDecision {

		readonly InvaderGroup invaderGroup;
		readonly int count;
		readonly string[] labels;

		public SelectInvadersToPush(InvaderGroup invaderGroup,int count,params string[] labels){
			this.invaderGroup = invaderGroup;
			this.count = count;
			this.labels = labels;

			// can't calculate Options yet because 
			// when pushing multiple invaders from target land
			// must wait for child descision (target land) to complete
			// and remove invader from invader group
			// before we can accurately calculate which are reamining.
		}

		public string Prompt => $"Select invader to push.";

		public IOption[] Options =>
			invaderGroup
				.InvaderTypesPresent
				.Where(i=>labels.Contains(i.Label))
				.ToArray();

		public void Select( IOption option, ActionEngine engine ) {

			// if we need more, push next
			if(count>1)
				engine.decisions.Push(new SelectInvadersToPush(invaderGroup,count-1,labels));


			// select where to push this invader
			engine.decisions.Push( new SelectInvaderDestination( invaderGroup, (Invader)option ) );
		}

	}

}
