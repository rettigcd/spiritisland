namespace SpiritIsland;

// Commands that act on: GameState
using GameCtxCmd = DecisionOption<GameCtx>;

public static partial class Cmd {

	static public GameCtxCmd AtTheStartOfNextRound( DecisionOption<GameState> cmd ) => new GameCtxCmd(
		"At the start of next round, "+cmd.Description,
		gs => gs.GameState.TimePasses_ThisRound.Push( cmd.Execute )	// There are no actions here, just game reconfig
	);

	static public GameCtxCmd AtTheStartOfEachInvaderPhase( GameCtxCmd cmd ) => new GameCtxCmd(
		"At the start of each Invader Phase, " + cmd.Description,
		ctx => ctx.GameState.StartOfInvaderPhase.ForGame.Add( ( _ ) => cmd.Execute( ctx ) )
	);

	static public GameCtxCmd OnEachBoard( this DecisionOption<BoardCtx> boardAction )
		=> new GameCtxCmd(
			"On each board, " + boardAction.Description,
			async ctx => {
				var gs = ctx.GameState;
				for(int boardIndex = 0; boardIndex < gs.Island.Boards.Length; ++boardIndex) {
					BoardCtx boardCtx = new BoardCtx( BoardCtx.FindSpirit( gs, boardIndex ), gs, gs.Island.Boards[boardIndex], ctx.ActionScope );
					await boardAction.Execute( boardCtx );
				}
			}
		);

	static public GameCtxCmd InEachLand( IExecuteOn<TargetSpaceCtx> action, Func<SpaceState, bool> filter = null )
		=> new GameCtxCmd(
			"In each land, " + action.Description,
			async ctx => {
				var gs = ctx.GameState;
				for(int i = 0; i < gs.Island.Boards.Length; ++i) {
					var decisionMaker = gs.Spirits[i < gs.Spirits.Length ? i : 0].BindSelf( gs, ctx.ActionScope ); // use Head spirit for extra board
					var board = gs.Island.Boards[i];
					var spaces = board.Spaces
						.Select( s => gs.Tokens[s] )
						.Where( x => filter == null || filter( x ) );
					foreach(var ss in spaces)
						await action.Execute( decisionMaker.Target( ss.Space ) );
				}
			}
		);

	static public DecisionOption<GameCtx> EachSpirit( DecisionOption<SelfCtx> action )
		=> new GameCtxCmd(
			"For each spirit, " + action.Description,
			async ctx => {
				foreach(Spirit spirit in ctx.GameState.Spirits)
					await action.Execute( spirit.BindSelf( ctx.GameState, ctx.ActionScope ) );
			}
		);

}