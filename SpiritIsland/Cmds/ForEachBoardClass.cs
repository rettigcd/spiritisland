namespace SpiritIsland;

using System.Threading.Tasks;

/// <summary>
/// Exectued a Board Action on each Board
/// </summary>
public class ForEachBoardClass : BaseCmd<GameState> {

	public ForEachBoardClass( IActOn<BoardCtx> boardAction ) : base() {
		_boardAction = boardAction;
	}

	public override string Description => $"On each board{FilterSuffix}: {_boardAction.Description}";

	public override async Task ActAsync( GameState gameState ) {
		var parentScope = ActionScope.Current;
		for(int boardIndex = 0; boardIndex < gameState.Island.Boards.Length; ++boardIndex) {
			BoardCtx boardCtx = new BoardCtx( gameState.Island.Boards[boardIndex] );
			for(int i = 0; i < boardCtx.Board.InvaderActionCount; ++i) {
				// Page 10 of JE says Each Board is a new action
				await using ActionScope actionScope = await ActionScope.Start( parentScope.Category );
				await _boardAction.ActAsync( boardCtx );
			}
		}
	}

	public ForEachBoardClass Which( CtxFilter<BoardCtx> filter ) {
		_filter = filter;
		return this;
	}

	#region private

	string FilterSuffix => _filter is null ? string.Empty : $" which {_filter.Description}";

	CtxFilter<BoardCtx> _filter;
	readonly IActOn<BoardCtx> _boardAction;

	#endregion
}
