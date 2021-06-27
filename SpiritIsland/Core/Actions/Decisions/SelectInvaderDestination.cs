using System.Linq;

namespace SpiritIsland.Core {
	public class SelectInvaderDestination : IDecision {

		readonly InvaderGroup invaderGroup;
		readonly Invader invader;

		public SelectInvaderDestination(InvaderGroup invaderGroup, Invader invader){
			this.invaderGroup = invaderGroup;
			this.invader = invader;
		}

		public IOption[] Options => invaderGroup.Space.SpacesExactly(1)
			.Where(x=>x.IsLand)
			.ToArray();

		public void Select( IOption option,ActionEngine engine) {
			engine.actions.Add(new MoveInvader(invader, invaderGroup.Space, (Space)option));
			invaderGroup[invader]--;
		}

		public string Prompt => $"Select land to push {invader.Summary} to.";


	}

}
