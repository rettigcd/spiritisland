namespace SpiritIsland.JaggedEarth;

public class GainTime : SpiritAction {

	readonly int _delta;

	public GainTime(int delta):base( $"GainTime({delta})" ) { _delta = delta; }

	public override async Task ActAsync( SelfCtx ctx ) {
		if(ctx.Self is FracturedDaysSplitTheSky fracturedDays)
			await fracturedDays.GainTime(_delta);
	}

}