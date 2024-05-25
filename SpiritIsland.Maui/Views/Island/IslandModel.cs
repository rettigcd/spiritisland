namespace SpiritIsland.Maui;

public class IslandModel {

	#region constructor

	public IslandModel(GameState gameState, OptionViewManager ovm) {
		ActionScope.ThrowIfMissingCurrent();

		_tokens = gameState.Tokens;
		_ovm = ovm;

		BoundsBuilder bb = new BoundsBuilder();
		_spaceModels = gameState.SpaceSpecs_Unfiltered
			.Select(s => {
				var layout = s.Layout;
				bb.Include(layout.Bounds);
				return new SpaceModel(_tokens[s], layout, _ovm);
			})
			.ToList();
		WorldBounds = bb.GetBounds();
	}

	#endregion constructor

	/// <summary> Sync the view model to the GameState </summary>
	public void Sync() {
		ActionScope.ThrowIfMissingCurrent();
		foreach (SpaceModel spaceModel in _spaceModels)
			spaceModel.Sync();
	}

	public void ClearIslandTokens() {
		// !!! Make Island responsd PROPERLY to spaces being added and removed,
		// then we don't have explicitly clear the tokens
		foreach (SpaceModel spaceModel in _spaceModels)
			spaceModel.ClearTokens();
		_spaceModels.Clear();
	}

	public Bounds WorldBounds { get; private set; }

	public readonly List<SpaceModel> _spaceModels = [];

	readonly Tokens_ForIsland _tokens = new Tokens_ForIsland();
	readonly OptionViewManager _ovm;
}
