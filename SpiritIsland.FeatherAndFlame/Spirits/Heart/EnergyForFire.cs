namespace SpiritIsland.FeatherAndFlame;

public class EnergyForFire : SpiritAction {

	public EnergyForFire() : base( "EnergyForFire" ) { }

	public override Task ActAsync( Spirit self ) {
		self.Energy += self.Presence.TrackElements[Element.Fire];
		return Task.CompletedTask;
	}

}