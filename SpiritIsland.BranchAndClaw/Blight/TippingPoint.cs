namespace SpiritIsland.BranchAndClaw;

public class TippingPoint : BlightCardBase {

	public TippingPoint():base("Tipping Point", "Immediately, destroy 3 presence from each Spirit.", 5 ) { }

	public override DecisionOption<GameCtx> Immediately 
		=> Cmd.ForEachSpirit( 
			// destroys 3 presence
			Cmd.DestroyPresence(3)
		);

}
