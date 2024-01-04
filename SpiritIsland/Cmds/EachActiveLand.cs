namespace SpiritIsland;

/// <summary>
/// Action occurs in each Active land.  No picking required.
/// </summary>
public class EachActiveLand : IActOn<GameState> {

	#region constructor

	public EachActiveLand( IActOn<TargetSpaceCtx> spaceAction, string preposition ) {
		_preposition = preposition;
		_spaceAction = spaceAction;
	}

	#endregion constructor

	public string Description => $"{_spaceAction.Description} {_preposition} each space {_landCriteria.Description}";

	public async Task ActAsync( GameState gameState ) {

		foreach(Board board in gameState.Island.Boards) {
			Spirit spirit = board.FindSpirit();
			var unfiltered = board.Spaces.Select( spirit.Target );
			var filtered = _landCriteria.Filter( unfiltered );
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

	readonly IActOn<TargetSpaceCtx> _spaceAction;
	readonly string _preposition;
	CtxFilter<TargetSpaceCtx> _landCriteria = CtxFilter<TargetSpaceCtx>.NullFilter;

	#endregion

}