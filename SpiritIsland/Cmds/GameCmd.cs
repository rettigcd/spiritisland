namespace SpiritIsland;

// Commands that act on: GameState
using GameCmd = ActionOption<GameState>;

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
	static public GameCmd OnEachBoard( this ActionOption<BoardCtx> boardAction ) 
		=> new GameCmd( 
			"On each board, " + boardAction.Description, 
			async gs => {
				for(int i = 0; i < gs.Spirits.Length; ++i) {
					BoardCtx boardCtx = new BoardCtx( gs.Spirits[i], gs, gs.Island.Boards[i] );
					await boardAction.Execute( boardCtx );
				}
			}
		);

	// !!! if this is on a per-space basis, shouldn't need a spirit / cause.
	static public GameCmd InEachLand( Cause cause, SpaceAction action, Func<TokenCountDictionary,bool> filter )
		=> new GameCmd(
			"In each land, " + action.Description, 
			async gs => {
				var decisionMaker = new SelfCtx(gs.Spirits[0],gs,cause);
				foreach(var space in gs.Island.AllSpaces.Where(x=>filter==null || filter(gs.Tokens[x]) ) )
					await action.Execute( decisionMaker.Target(space) );
			}
		);

	static public ActionOption<GameState> EachSpirit( Cause cause, ActionOption<SelfCtx> action )
		=> new GameCmd(
			"For each spirit, " + action.Description, 
			async gs => {
				foreach(var spiritCtx in gs.SpiritCtxs(cause) )
					await action.Execute( spiritCtx );
			}
		);

}