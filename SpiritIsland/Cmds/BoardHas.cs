namespace SpiritIsland;

/// <summary>
/// Contains Board Filters
/// </summary>
static public class BoardHas {

	static public CtxFilter<BoardCtx> TownOrCity => new CtxFilter<BoardCtx>(
		"has town/city",
		b => b.Board.Spaces
			.Where( TerrainMapper.Current.IsInPlay )
			.ScopeTokens()
			.Any( s => s.HasAny( Human.Town_City ) )
	);

}
