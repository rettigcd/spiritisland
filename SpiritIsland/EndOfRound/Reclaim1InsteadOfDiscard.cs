namespace SpiritIsland;

public class Reclaim1InsteadOfDiscard : IRunWhenTimePasses {

	readonly Spirit spirit;
	readonly PowerCard[] _purchased;

	public Reclaim1InsteadOfDiscard( Spirit spirit ) {
		this.spirit = spirit;
		_purchased = [.. spirit.InPlay];
	}

	// Used by FromJson to restore the exact captured snapshot, which may no longer match the target
	// spirit's current InPlay by the time a game is restored.
	Reclaim1InsteadOfDiscard( Spirit spirit, PowerCard[] purchased ) {
		this.spirit = spirit;
		_purchased = purchased;
	}

	async Task IRunWhenTimePasses.TimePasses( GameState _ ) {
		var reclaimCard = await spirit.SelectPowerCard( "Reclaim 1 played card", 1, _purchased, CardUse.Reclaim, Present.Done );
		if(reclaimCard != null)
			spirit.Reclaim(reclaimCard);
	}
	bool IRunWhenTimePasses.RemoveAfterRun => true;
	TimePassesOrder IRunWhenTimePasses.Order => TimePassesOrder.Early;


}
