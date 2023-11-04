namespace SpiritIsland.NatureIncarnate;

public class GainEnergyAnAdditionalTime : SpiritAction, ICanAutoRun {

	public GainEnergyAnAdditionalTime():base( "Gain Energy an additional time" ) { }

	public override Task ActAsync( SelfCtx ctx ) {
		var spirit = (RelentlessGazeOfTheSun)ctx.Self;
		spirit.CollectEnergySecondTime = true;
		return Task.CompletedTask;
	}
}
