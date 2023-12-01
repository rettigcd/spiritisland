namespace SpiritIsland.NatureIncarnate;

public class GainEnergyAnAdditionalTime : SpiritAction, ICanAutoRun {

	public GainEnergyAnAdditionalTime():base( "Gain Energy an additional time" ) { }

	public override Task ActAsync( Spirit self ) {
		var spirit = (RelentlessGazeOfTheSun)self;
		spirit.CollectEnergySecondTime = true;
		return Task.CompletedTask;
	}
}
