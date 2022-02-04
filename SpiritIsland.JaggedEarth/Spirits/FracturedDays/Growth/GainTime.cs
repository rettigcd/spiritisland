namespace SpiritIsland.JaggedEarth;

public class GainTime : GrowthActionFactory {

	readonly int delta;

	public GainTime(int delta) { this.delta = delta; }

	public override async Task ActivateAsync( SelfCtx ctx ) {
		if(ctx.Self is FracturedDaysSplitTheSky fracturedDays)
			await fracturedDays.GainTime( delta, ctx.GameState );
	}

	public override string Name => $"GainTime({delta})";

}