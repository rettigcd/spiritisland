namespace SpiritIsland;

public class Reclaim1InsteadOfDiscard : IRunWhenTimePasses {

	readonly Spirit _spirit;
	readonly PowerCard[] _purchased;

	public Reclaim1InsteadOfDiscard( Spirit spirit ) {
		_spirit = spirit;
		_purchased = [..spirit.InPlay]; // make copy in case spirit is cleaned up before this is called
	}

	async Task IRunWhenTimePasses.TimePasses( GameState _ ) {
		var reclaimCard = await _spirit.SelectPowerCard( "Reclaim 1 played card", _purchased, CardUse.Reclaim, Present.Done );
		if(reclaimCard != null)
			_spirit.Reclaim(reclaimCard);
	}
	bool IRunWhenTimePasses.RemoveAfterRun => true;
	TimePassesOrder IRunWhenTimePasses.Order => TimePassesOrder.Early;

}