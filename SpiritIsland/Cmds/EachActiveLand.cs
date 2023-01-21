namespace SpiritIsland;

/// <summary>
/// Action occurs in each Active land.  No picking required.
/// </summary>
public class EachActiveLand : IExecuteOn<GameCtx> {

	static readonly TargetSpaceCtxFilter AnyLand = new TargetSpaceCtxFilter( "any land", _ => true );

	readonly IExecuteOn<TargetSpaceCtx> _spaceAction;
	readonly string _preposition;
	TargetSpaceCtxFilter _landCriteria = AnyLand;

	public EachActiveLand( IExecuteOn<TargetSpaceCtx> spaceAction, string preposition ) {
		_preposition = preposition;
		_spaceAction = spaceAction;
	}
	public string Description => $"{_spaceAction.Description} {_preposition} each space {_landCriteria.Description}";

	public async Task Execute( GameCtx ctx ) {
		var gameState = ctx.GameState;
		for(int i = 0; i < gameState.Island.Boards.Length; ++i) {
			SelfCtx decisionMaker = gameState.Spirits[i < gameState.Spirits.Length ? i : 0].BindSelf( gameState, ctx.ActionScope ); // use Head spirit for extra board - !!! this already defined somewhere
			Board board = gameState.Island.Boards[i];
			var spacesCtxs = ctx.GameState.Tokens
				.PowerUp( board.Spaces )
				.Where( ctx.ActionScope.TerrainMapper.IsInPlay )
				.Select( decisionMaker.Target )
				.Where( _landCriteria.Filter );
			foreach(var ss in spacesCtxs)
				await _spaceAction.Execute( ss );
		}

	}

	// OnEach does not have a ByPickingToken option because we don't pick a space, we do it on ALL spaces.

	public EachActiveLand Which( TargetSpaceCtxFilter filter ) {
		_landCriteria = filter;
		return this;
	}

	public bool IsApplicable( GameCtx ctx ) => true;
}