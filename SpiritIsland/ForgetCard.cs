namespace SpiritIsland;

// end of round action
// used by: Unlock the Gates of Deepest power
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