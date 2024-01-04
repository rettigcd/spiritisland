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

		var unfiltered = gameState.Island.Boards
			.Select( board => new BoardCtx( board ) );
		var applicableBoards = _filter.Filter( unfiltered );

		foreach(BoardCtx boardCtx in applicableBoards) {
			// Page 10 of JE says Each Board is a new action
			await using ActionScope actionScope = await ActionScope.Start( parentScope.Category );
			await _boardAction.ActAsync( boardCtx );
		}

	}

	public ForEachBoardClass Which( CtxFilter<BoardCtx> filter ) {
		_filter = filter;
		return this;
	}

	#region private

	string FilterSuffix => _filter is null ? string.Empty : $" which {_filter.Description}";

	CtxFilter<BoardCtx> _filter = CtxFilter<BoardCtx>.NullFilter;
	readonly IActOn<BoardCtx> _boardAction;

	#endregion
}
