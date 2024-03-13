namespace SpiritIsland.Maui;

public class IslandModel {

	#region constructor

	public IslandModel(Board board, Tokens_ForIsland tokens, OptionViewManager ovm) {
		ActionScope.ThrowIfMissingCurrent();

		_board = board;
		_tokens = tokens;
		_ovm = ovm;

		_boardLayout = BoardLayout.Get(_board.Name);

		_spaceModels = _board.Spaces_Unfiltered
			.Select(s => new SpaceModel(_tokens[s], _boardLayout.ForSpace(s), _ovm))
			.ToList();
	}

	#endregion constructor

	public void Sync() {
		ActionScope.ThrowIfMissingCurrent();
		foreach (SpaceModel spaceModel in _spaceModels)
			spaceModel.Sync();
	}

	public void ClearIslandTokens() {
		// !!! Make Island response PROPERLY to spaces being added and removed,
		// then we don't have explicitly clear the tokens
		foreach (SpaceModel spaceModel in _spaceModels)
			spaceModel.ClearTokens();
		_spaceModels.Clear();
	}

	public readonly Board _board;
	public readonly BoardLayout _boardLayout;
	public readonly List<SpaceModel> _spaceModels = [];

	readonly Tokens_ForIsland _tokens = new Tokens_ForIsland();
	readonly OptionViewManager _ovm;
}
