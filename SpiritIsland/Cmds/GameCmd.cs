namespace SpiritIsland;

// Commands that act on: GameState
using GameCmd = DecisionOption<GameState>;
using GameCtxCmd = DecisionOption<GameCtx>;

public static partial class Cmd {

	static public GameCtxCmd AtTheStartOfNextRound( GameCmd cmd ) => new GameCtxCmd(
		"At the start of next round, "+cmd.Description,
		gs => gs.GameState.TimePasses_ThisRound.Push( cmd.Execute )	// !!! we lose ActionCategory here.  Is that ok?
	);

	static public GameCtxCmd AtTheStartOfEachInvaderPhase( GameCtxCmd cmd ) => new GameCtxCmd(
		"At the start of each Invader Phase, " + cmd.Description,
		ctx => ctx.GameState.StartOfInvaderPhase.ForGame.Add( ( _ ) => cmd.Execute( ctx ) )
	);

	// GameState actions
	// Deprecated. Use OnEachBoard
	static public GameCmd OnEachBoardOld( this DecisionOption<BoardCtx> boardAction ) 
		=> new GameCmd( 
			"On each board, " + boardAction.Description, 
			async gs => {

				UnitOfWork actionId = gs.StartAction( ActionCategory.Fear ); // !!! WRONG - this is used for Fear, Blight and possibly SpiritPowers
				// This needs passed in so that Fear/Blight with multiple steps, can all use the same unit-of-work

				for(int i = 0; i < gs.Spirits.Length; ++i) {
					BoardCtx boardCtx = new BoardCtx( gs.Spirits[i < gs.Spirits.Length ? i : 0], gs, gs.Island.Boards[i], actionId );
					await boardAction.Execute( boardCtx );
				}
			}
		);

	static public GameCtxCmd OnEachBoard( this DecisionOption<BoardCtx> boardAction )
		=> new GameCtxCmd(
			"On each board, " + boardAction.Description,
			async ctx => {
				var gs = ctx.GameState;
				for(int i = 0; i < gs.Spirits.Length; ++i) {
					BoardCtx boardCtx = new BoardCtx( gs.Spirits[i < gs.Spirits.Length ? i : 0], gs, gs.Island.Boards[i], ctx.UnitOfWork );
					await boardAction.Execute( boardCtx );
				}
			}
		);

	// Even though this is on a per-space basis, let spirit that started on board be the decision maker.
	/// <summary>
	/// Used ONLY for Fear Actions. 
	/// DO NOT use for Spirit Powers or Rituals without setting cause = Cause.Power
	/// </summary>
	static public GameCtxCmd InEachLand( IExecuteOn<TargetSpaceCtx> action, Func<SpaceState, bool> filter = null )
		=> new GameCtxCmd(
			"In each land, " + action.Description,
			async ctx => {
				var gs = ctx.GameState;
				for(int i = 0; i < gs.Island.Boards.Length; ++i) {
					var decisionMaker = gs.Spirits[i < gs.Spirits.Length ? i : 0].Bind( gs, ctx.UnitOfWork ); // use Head spirit for extra board
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
	static public DecisionOption<GameCtx> EachSpirit( DecisionOption<SelfCtx> action )
		=> new GameCtxCmd(
			"For each spirit, " + action.Description,
			async ctx => {
				var gs = ctx.GameState;
				foreach(var spirit in gs.Spirits)
					await action.Execute( spirit.Bind( gs, ctx.UnitOfWork ) );
			}
		);

}