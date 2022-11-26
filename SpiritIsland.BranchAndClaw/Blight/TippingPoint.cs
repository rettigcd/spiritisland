namespace SpiritIsland.BranchAndClaw;

public class TippingPoint : BlightCardBase {

	public TippingPoint():base("Tipping Point", 5 ) { }

	public override DecisionOption<GameState> Immediately 
		=> Cmd.EachSpirit( 
			// destroys 3 presence
			Cmd.DestroyPresence(3,DestoryPresenceCause.BlightedIsland)
		);

}
