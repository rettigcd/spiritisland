namespace SpiritIsland;

public class GainEnergyEqualToCardPlays : SpiritAction {
	public GainEnergyEqualToCardPlays() : base( "Gain Energy Equal to Card Plays" ) { }
	public override Task ActAsync( Spirit self ) {
		self.Energy += self.Presence.CardPlayPerTurn;
		return Task.CompletedTask;
	}
}