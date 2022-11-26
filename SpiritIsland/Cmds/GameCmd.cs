namespace SpiritIsland;

// Commands that act on: GameState
using GameCmd = DecisionOption<GameState>;

public static partial class Cmd {

	static public GameCmd AtTheStartOfNextRound( GameCmd cmd ) => new GameCmd(
		"At the start of next round, "+cmd.Description,
		gs => gs.TimePasses_ThisRound.Push( cmd.Execute )
	);

	static public GameCmd AtTheStartOfEachInvaderPhase( GameCmd cmd ) => new GameCmd(
		"At the start of each Invader Phase, "+cmd.Description,
		gs => gs.StartOfInvaderPhase.ForGame.Add( ( gs ) => cmd.Execute(gs) )
	);

	// GameState actions
	static public GameCmd OnEachBoard( this DecisionOption<BoardCtx> boardAction ) 
		=> new GameCmd( 
			"On each board, " + boardAction.Description, 
			async gs => {
				Guid actionId = Guid.NewGuid();
				for(int i = 0; i < gs.Spirits.Length; ++i) {
					BoardCtx boardCtx = new BoardCtx( gs.Spirits[i < gs.Spirits.Length ? i : 0], gs, gs.Island.Boards[i], actionId );
					await boardAction.Execute( boardCtx );
				}
			}
		);

	// Even though this is on a per-space basis, let spirit that started on board be the decision maker.
	/// <summary>
	/// Used ONLY for Fear Actions. 
	/// DO NOT use for Spirit Powers or Rituals without setting cause = Cause.Power
	/// </summary>
	static public GameCmd InEachLand( IExecuteOn<TargetSpaceCtx> action, Func<SpaceState,bool> filter = null )
		=> new GameCmd(
			"In each land, " + action.Description, 
			async gs => {
				for(int i = 0; i < gs.Island.Boards.Length; ++i) {
					var actionId = Guid.NewGuid(); // different action on each space
					var decisionMaker = gs.Spirits[i < gs.Spirits.Length ? i : 0].Bind( gs, actionId ); // use Head spirit for extra board
					var board = gs.Island.Boards[i];
					var spaces = board.Spaces
						.Select( s => gs.Tokens[s] )
						.Where( s => !s.InStasis )
						.Where( x => filter == null || filter( x ) );
					foreach(var ss in spaces)
						await action.Execute( decisionMaker.Target( ss.Space ) );
				}
			}
		);

	/// <summary>
	/// Used ONLY for Fear Actions. 
	/// DO NOT use for Spirit Powers or Rituals without setting cause = Cause.Power
	/// </summary>
	static public DecisionOption<GameState> EachSpirit( DecisionOption<SelfCtx> action )
		=> new GameCmd(
			"For each spirit, " + action.Description, 
			async gs => {
				var actionId = Guid.NewGuid();
				foreach(var spirit in gs.Spirits)
					await action.Execute( spirit.Bind( gs, actionId ) );
			}
		);

}