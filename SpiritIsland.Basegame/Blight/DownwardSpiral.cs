namespace SpiritIsland.Basegame;

public class DownwardSpiral : BlightCardBase {

	public DownwardSpiral():base("Downward Spiral",5) {}

	public override DecisionOption<GameCtx> Immediately 
		=> Cmd.AtTheStartOfEachInvaderPhase(
			Cmd.EachSpirit(
				Cmd.DestroyPresence(DestoryPresenceCause.BlightedIsland) 
			) 
		);

}