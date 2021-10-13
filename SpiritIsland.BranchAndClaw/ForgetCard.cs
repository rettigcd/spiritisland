using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	// end of round action
	public class ForgetCard {

		readonly Spirit spirit;
		readonly PowerCard card;

		public ForgetCard(Spirit spirit, PowerCard card ) {
			this.spirit = spirit;
			this.card = card;
		}

		public Task Forget( GameState _ ) {
			spirit.Forget( card );
			return Task.CompletedTask;
		}
	}

}
