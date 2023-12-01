namespace SpiritIsland;

public class GainEnergyEqualToCardPlays : SpiritAction {
	public GainEnergyEqualToCardPlays() : base( "GainEnergyEqualToCardPlays" ) { }
	public override Task ActAsync( Spirit self ) {
		self.Energy += self.Presence.CardPlayPerTurn;
		return Task.CompletedTask;
	}
}