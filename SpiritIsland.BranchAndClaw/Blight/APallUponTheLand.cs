namespace SpiritIsland.BranchAndClaw;

public class APallUponTheLand : BlightCardBase {

	public APallUponTheLand():base("A Pall Upon the Land", "Immediately, on each board: destroy 1 presence and remove 1 town.", 3 ) { }

	public override DecisionOption<GameCtx> Immediately => 
		// On Each Board
		Cmd.ForEachBoard(
			Cmd.Multiple(
				// destroy 1 presence 
				Destroy1PresenceOnBoard,
				// remove 1 town
				Cmd.RemoveTowns(1).In().OneLandPerBoard()
			)
		);

	static DecisionOption<BoardCtx> Destroy1PresenceOnBoard 
		=> new DecisionOption<BoardCtx>("Destroy 1 presence.", async boardCtx => {
			// !! this could be any spirits presence - Who picks?   When having multiple players, this should be a parallel decision where spirit volunteers
			// for now, just have the 1st spirit pick (House Rules advantage)

			bool IsOnBoard(Spirit spirit) => boardCtx.ActiveSpaces.Any(spirit.Presence.IsOn);
			var spiritOptions = boardCtx.GameState.Spirits
				.Where( IsOnBoard )
				.ToArray();
			if(spiritOptions.Length == 0) return;
			var spirit = spiritOptions.Length == 1 ? spiritOptions[0]
				: await boardCtx.Decision( new Select.Spirit( "Destroy 1 presence.", spiritOptions ) );
			await spirit.DestroyOnePresenceFromAnywhere( s=>s.Space.Board == boardCtx.Board );
		} );

}