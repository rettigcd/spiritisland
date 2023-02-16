namespace SpiritIsland;

// Commands that act on: GameState
using GameCtxCmd = DecisionOption<GameCtx>;

public static partial class Cmd {

	// Various "For" prepositions
	static public (IExecuteOn<TargetSpaceCtx> action, string preposition) In( this IExecuteOn<TargetSpaceCtx> spaceAction ) => (spaceAction, "in");
	static public (IExecuteOn<TargetSpaceCtx> action, string preposition) From( this IExecuteOn<TargetSpaceCtx> spaceAction ) => (spaceAction, "from");
	static public (IExecuteOn<TargetSpaceCtx> action, string preposition) To( this IExecuteOn<TargetSpaceCtx> spaceAction ) => (spaceAction, "to");
	static public (IExecuteOn<TargetSpaceCtx> action, string preposition) On( this IExecuteOn<TargetSpaceCtx> spaceAction ) => (spaceAction, "on");

	// How we select the land
	// !!! Page 10 of JE says Each Land (Spirit picked or otherwise) is a new action
	static public SpiritPicksLandAction SpiritPickedLand( this (IExecuteOn<TargetSpaceCtx> spaceAction, string preposition) x ) => new SpiritPicksLandAction( x.spaceAction, x.preposition );
	static public EachActiveLand EachActiveLand( this (IExecuteOn<TargetSpaceCtx> spaceAction, string preposition) x ) => new EachActiveLand( x.spaceAction, x.preposition );
	static public NLandsPerBoard OneLandPerBoard( this (IExecuteOn<TargetSpaceCtx> spaceAction, string preposition) x ) => new NLandsPerBoard( x.spaceAction, x.preposition, 1 );
	static public NLandsPerBoard NDifferentLands( this (IExecuteOn<TargetSpaceCtx> spaceAction, string preposition) x, int count ) => new NLandsPerBoard( x.spaceAction, x.preposition, count );

	// For each: Board
	static public GameCtxCmd ForEachBoard( this IExecuteOn<BoardCtx> boardAction )
		=> new GameCtxCmd(
			"On each board, " + boardAction.Description,
			async ctx => {
				var gs = ctx.GameState;
				for(int boardIndex = 0; boardIndex < gs.Island.Boards.Length; ++boardIndex) {
					BoardCtx boardCtx = new BoardCtx( gs, gs.Island.Boards[boardIndex] );
					for(int i = 0; i < boardCtx.Board.InvaderActionCount; ++i) {
						// Page 10 of JE says Each Board is a new action
						await using var actionScope = new ActionScope( ctx.Category );
						await boardAction.Execute( boardCtx );
					}
				}
			}
		);

	// For each: Spirit
	static public DecisionOption<GameCtx> ForEachSpirit( this IExecuteOn<SelfCtx> action )
		=> new GameCtxCmd(
			"For each spirit, " + action.Description,
			async ctx => {
				foreach(Spirit spirit in ctx.GameState.Spirits) {
					// Page 10 of JE says Each Spirit is a new action
					await using var actionScope = new ActionScope( ctx.Category );
					await action.Execute( spirit.BindSelf() );
				}
			}
		);

	// At specific times
	static public GameCtxCmd AtTheStartOfNextRound( this DecisionOption<GameState> cmd ) => new GameCtxCmd(
		"At the start of next round, " + cmd.Description,
		gs => gs.GameState.TimePasses_ThisRound.Push( cmd.Execute ) // There are no actions here, just game reconfig
	);

	static public GameCtxCmd AtTheStartOfEachInvaderPhase( this GameCtxCmd cmd ) => new GameCtxCmd(
		"At the start of each Invader Phase, " + cmd.Description,
		ctx => ctx.GameState.StartOfInvaderPhase.ForGame.Add( ( _ ) => cmd.Execute( ctx ) )
	);

}