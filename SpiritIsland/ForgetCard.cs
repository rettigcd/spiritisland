namespace SpiritIsland;

// end of round action
// used by: Unlock the Gates of Deepest power
public class ForgetCard( Spirit _spirit, PowerCard _card ) {
	public Task Forget( GameState _ ) {
		_spirit.ForgetThisCard( _card );
		return Task.CompletedTask;
	}
}