namespace SpiritIsland.JaggedEarth;

public class Pay2EnergyToGainAPowerCard : SpiritAction {

	public Pay2EnergyToGainAPowerCard():base( "Pay2EnergyToGainAPowerCard" ) { }

	public override async Task ActAsync( Spirit self ) {
		if( 2 <= self.Energy && await self.UserSelectsFirstText("Draw Power Card?", "Yes, pay 2 energy", "No, thank you.")) {
			self.Energy -= 2;
			await self.Draw.Card();
		}
	}

}