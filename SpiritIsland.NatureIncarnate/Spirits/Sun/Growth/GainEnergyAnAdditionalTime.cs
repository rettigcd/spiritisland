namespace SpiritIsland.NatureIncarnate;

public class GainEnergyAnAdditionalTime : SpiritAction, ICanAutoRun {

	public GainEnergyAnAdditionalTime():base( "Gain Energy an additional time" ) { }

	public override Task ActAsync(Spirit self) {
		_id = self.EnergyCollected.Add(GainEnergyAndRemove);
		return Task.CompletedTask;
	}

	void GainEnergyAndRemove(Spirit self) {
		self.Energy += self.EnergyPerTurn;
		self.EnergyCollected.Remove(_id);
	}

	Guid _id;
}
