namespace SpiritIsland;

/// <summary>
/// Action occurs in each Active land.  No picking required.
/// </summary>
public class EachActiveLand : IActOn<GameCtx> {

	readonly IActOn<TargetSpaceCtx> _spaceAction;
	readonly string _preposition;
	TargetSpaceCtxFilter _landCriteria = Is.AnyLand;

	public EachActiveLand( IActOn<TargetSpaceCtx> spaceAction, string preposition ) {
		_preposition = preposition;
		_spaceAction = spaceAction;
	}
	public string Description => $"{_spaceAction.Description} {_preposition} each space {_landCriteria.Description}";

	public async Task ActAsync( GameCtx ctx ) {
		var gameState = ctx.GameState;
		foreach(Board board in gameState.Island.Boards) {
			Spirit spirit = board.FindSpirit();
			SelfCtx decisionMaker = spirit.Bind();
			var spacesCtxs = board.Spaces.Tokens()
				.Select( decisionMaker.Target )
				.Where( _landCriteria.Filter );
			foreach(TargetSpaceCtx ss in spacesCtxs)
				for(int i=0;i<board.InvaderActionCount;++i)
					await _spaceAction.ActAsync( ss );
		}

	}

	// OnEach does not have a ByPickingToken option because we don't pick a space, we do it on ALL spaces.

	public EachActiveLand Which( TargetSpaceCtxFilter filter ) {
		_landCriteria = filter;
		return this;
	}

	public bool IsApplicable( GameCtx ctx ) => true;
}