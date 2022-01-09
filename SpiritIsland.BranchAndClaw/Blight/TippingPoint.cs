namespace SpiritIsland.BranchAndClaw {

	public class TippingPoint : BlightCardBase {

		public TippingPoint():base("Tipping Point", 5 ) { }

		public override ActionOption<GameState> Immediately 
			=> Cmd.EachSpirit( Cause.Blight,
				// destroys 3 presence
				Cmd.DestroyPresence(3,ActionType.BlightedIsland)
			);


	}
}
