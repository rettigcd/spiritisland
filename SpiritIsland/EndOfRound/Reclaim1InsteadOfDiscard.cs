namespace SpiritIsland;

public class Reclaim1InsteadOfDiscard( Spirit spirit ) : IRunWhenTimePasses {

	readonly PowerCard[] _purchased = [.. spirit.InPlay];

	async Task IRunWhenTimePasses.TimePasses( GameState _ ) {
		var reclaimCard = await spirit.SelectPowerCard( "Reclaim 1 played card", 1, _purchased, CardUse.Reclaim, Present.Done );
		if(reclaimCard != null)
			spirit.Reclaim(reclaimCard);
	}
	bool IRunWhenTimePasses.RemoveAfterRun => true;
	TimePassesOrder IRunWhenTimePasses.Order => TimePassesOrder.Early;

}