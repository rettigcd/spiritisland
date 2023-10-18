namespace SpiritIsland.BranchAndClaw;

public class TippingPoint : BlightCard {

	public TippingPoint():base("Tipping Point", "Immediately, destroy 3 presence from each Spirit.", 5 ) { }

	public override BaseCmd<GameCtx> Immediately 
		=> Cmd.ForEachSpirit( 
			// destroys 3 presence
			Cmd.DestroyPresence(3)
		);

}
