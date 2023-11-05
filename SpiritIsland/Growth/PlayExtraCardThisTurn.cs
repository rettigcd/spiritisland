namespace SpiritIsland;

public class PlayExtraCardThisTurn : SpiritAction, ICanAutoRun {

	public PlayExtraCardThisTurn( int count )
		:base(
			$"PlayExtraCardThisTurn({count})",
			ctx=>ctx.Self.tempCardPlayBoost += count
		)
	{}

}
