namespace SpiritIsland.BranchAndClaw;

public class APallUponTheLand : BlightCard {

	public APallUponTheLand():base("A Pall Upon the Land", "Immediately, on each board: destroy 1 presence and remove 1 town.", 3 ) { }

	public override IActOn<GameState> Immediately => 
		// On Each Board
		Cmd.ForEachBoard(
			Cmd.Multiple(
				// destroy 1 presence 
				Destroy1PresenceOnBoard,
				// remove 1 town
				Cmd.RemoveTowns(1).In().OneLandPerBoard()
			)
		);

	static BaseCmd<BoardCtx> Destroy1PresenceOnBoard 
		=> new BaseCmd<BoardCtx>("Destroy 1 presence.", async boardCtx => {
			// !! this could be any spirits presence - Who picks?   When having multiple players, this should be a parallel decision where spirit volunteers
			// for now, just have the 1st spirit pick (House Rules advantage)
			var gs = GameState.Current;

			bool IsOnBoard(Spirit spirit) => spirit.Presence.IsOn( boardCtx.Board );
			var spiritOptions = gs.Spirits
				.Where( IsOnBoard )
				.ToArray();
			if(spiritOptions.Length == 0) return;
			var spirit = spiritOptions.Length == 1 ? spiritOptions[0]
				: await boardCtx.SelectAsync( new A.Spirit( "Destroy 1 presence.", spiritOptions ) );
			var spaceToken = await spirit.SelectAsync( new A.SpaceToken( "Select Presence to Destory"
				, spirit.Presence.Deployed.WhereIsOn(boardCtx.Board.Spaces.ScopeTokens()), Present.Always ) );
			await spaceToken.Destroy();
		} );

}