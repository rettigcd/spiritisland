namespace SpiritIsland.NatureIncarnate;

public class AccelerateOrDelay : SpiritAction {

	public AccelerateOrDelay():base( "Accelerate or Delay" ) { }

	public override async Task ActAsync( Spirit self ) {
		var spirit = (DancesUpEarthquakes)self;
		var options = spirit.Impending.ToList();
		for(int i = 0; i < 2; ++i) {
			// Select card to bump or delay
			string prompt = $"Select impending card to accelerate or delay. ({i + 1}of 2)";
			var card = await spirit.SelectPowerCard(prompt,1,options,CardUse.Accept,Present.Done);
			if(card == null) break;
			options.Remove( card );

			// Bump or Delay?
			int dif = card.Cost - spirit.ImpendingEnergy[card.Title]; // -1 because of this round.
			bool accelerate = await spirit.UserSelectsFirstText( $"{card.Title} is {dif} away from maturity.", "Accelerate 1", "Delay 1" );
			// Save choice.
			spirit.ImpendingEnergy[card.Title] += accelerate ? 1 : -1;
		}
		await Task.Delay(0);
	}
}