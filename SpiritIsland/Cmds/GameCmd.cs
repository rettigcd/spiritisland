namespace SpiritIsland;


// Commands that act on: GameState
using GameStateAction = IActOn<GameState>;

public static partial class Cmd {

	// Various "For" prepositions
	static public (IActOn<TargetSpaceCtx> action, string preposition) In( this IActOn<TargetSpaceCtx> spaceAction ) => (spaceAction, "in");
	static public (IActOn<TargetSpaceCtx> action, string preposition) From( this IActOn<TargetSpaceCtx> spaceAction ) => (spaceAction, "from");
	static public (IActOn<TargetSpaceCtx> action, string preposition) To( this IActOn<TargetSpaceCtx> spaceAction ) => (spaceAction, "to");
	static public (IActOn<TargetSpaceCtx> action, string preposition) On( this IActOn<TargetSpaceCtx> spaceAction ) => (spaceAction, "on");

	// How we select the land
	// Page 10 of JE says Each Land (Spirit picked or otherwise) is a new action
	static public SpiritPicksLandAction SpiritPickedLand( this (IActOn<TargetSpaceCtx> spaceAction, string preposition) x ) => new SpiritPicksLandAction( x.spaceAction, x.preposition );
	static public EachActiveLand EachActiveLand( this (IActOn<TargetSpaceCtx> spaceAction, string preposition) x ) => new EachActiveLand( x.spaceAction, x.preposition );
	static public NLandsPerBoard OneLandPerBoard( this (IActOn<TargetSpaceCtx> spaceAction, string preposition) x ) => new NLandsPerBoard( x.spaceAction, x.preposition, 1 );
	static public NLandsPerBoard NDifferentLandsPerBoard( this (IActOn<TargetSpaceCtx> spaceAction, string preposition) x, int count ) => new NLandsPerBoard( x.spaceAction, x.preposition, count );

	// For each: Board
	static public GameStateAction ForEachBoard( this IActOn<BoardCtx> boardAction )
		=> new BaseCmd<GameState>(
			"On each board, " + boardAction.Description,
			async gs => {

				var parentScope = ActionScope.Current;
				for(int boardIndex = 0; boardIndex < gs.Island.Boards.Length; ++boardIndex) {
					BoardCtx boardCtx = new BoardCtx( gs.Island.Boards[boardIndex] );
					for(int i = 0; i < boardCtx.Board.InvaderActionCount; ++i) {
						// Page 10 of JE says Each Board is a new action
						await using ActionScope actionScope = await ActionScope.Start( parentScope.Category );
						await boardAction.ActAsync( boardCtx );
					}
				}
			}
		);

	// For each: Spirit
	static public EachSpirit ForEachSpirit( this IActOn<Spirit> action ) => new EachSpirit(action);

	// At specific times
	static public GameStateAction AtTheStartOfNextRound( this GameStateAction cmd ) => new BaseCmd<GameState>(
		"At the start of next round, " + cmd.Description,
		gs => gs.TimePasses_ThisRound.Push( cmd.ActAsync ) // There are no actions here, just game reconfig
	);

	static public GameStateAction AtTheStartOfEachInvaderPhase( this GameStateAction cmd ) => new BaseCmd<GameState>(
		"At the start of each Invader Phase, " + cmd.Description,
		gs => gs.StartOfInvaderPhase.Add( ( _ ) => cmd.ActAsync( gs ) )
	);

}
