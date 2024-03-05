namespace SpiritIsland.FeatherAndFlame;

public class EnergyForFire : SpiritAction {

	public EnergyForFire() : base( "Energy for Fire" ) { }

	public override Task ActAsync( Spirit self ) {
		self.Energy += self.Presence.TrackElements[Element.Fire];
		return Task.CompletedTask;
	}

}