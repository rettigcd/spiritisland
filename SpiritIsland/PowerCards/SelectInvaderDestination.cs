using System.Linq;

namespace SpiritIsland.PowerCards {
	public class SelectInvaderDestination : IDecision {

		readonly InvaderGroup invaderGroup;
		readonly Invader invader;
		readonly Space from;

		public SelectInvaderDestination(InvaderGroup invaderGroup, Invader invader, Space from){
			this.invaderGroup = invaderGroup;
			this.invader = invader;
			this.from = from;
		}

		public IOption[] Options => from.SpacesExactly(1)
			.Where(x=>x.IsLand)
			.ToArray();

		public void Select( IOption option,ActionEngine engine) {
			engine.moves.Add(new MoveInvader(invader, from, (Space)option));
			invaderGroup[invader]--;
		}

	}

}
