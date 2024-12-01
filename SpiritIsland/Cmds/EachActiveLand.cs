namespace SpiritIsland;

/// <summary>
/// Action occurs in each Active land.  No picking required.
/// </summary>
public class EachActiveLand( IActOn<TargetSpaceCtx> _spaceAction, string _preposition ) : IActOn<GameState> {

	#region constructor

	#endregion constructor

	public string Description => $"{_spaceAction.Description} {_preposition} each space {_landCriteria.Description}";

	public async Task ActAsync( GameState gameState ) {

		foreach(Board board in gameState.Island.Boards) {
			Spirit spirit = board.FindSpirit();
			var unfiltered = board.Spaces.Select( spirit.Target ).ToArray();
			var filtered = _landCriteria.Filter( unfiltered ).ToArray();
			foreach(TargetSpaceCtx ss in filtered)
				for(int i=0;i<board.InvaderActionCount;++i)
					await _spaceAction.ActAsync( ss );
		}

	}

	// OnEach does not have a ByPickingToken option because we don't pick a space, we do it on ALL spaces.

	public EachActiveLand Which( CtxFilter<TargetSpaceCtx> filter ) {
		_landCriteria = filter;
		return this;
	}

	public bool IsApplicable( GameState _ ) => true;

	#region private
	CtxFilter<TargetSpaceCtx> _landCriteria = CtxFilter<TargetSpaceCtx>.NullFilter;

	#endregion

}