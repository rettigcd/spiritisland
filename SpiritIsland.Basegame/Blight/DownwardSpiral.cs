namespace SpiritIsland.Basegame;

public class DownwardSpiral : BlightCardBase {

	public DownwardSpiral():base("Downward Spiral", "At the start of each Invader Phase each Spirit destorys 1 of their presence.",5 ) {}

	public override DecisionOption<GameCtx> Immediately 
		=> Cmd.AtTheStartOfEachInvaderPhase(
			Cmd.EachSpirit(
				Cmd.DestroyPresence(DestoryPresenceCause.BlightedIsland) 
			) 
		);

}