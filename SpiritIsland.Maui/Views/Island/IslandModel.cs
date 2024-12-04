using System.Collections.ObjectModel;

namespace SpiritIsland.Maui;

public class IslandModel : ObservableModel {

	public ObservableCollection<SpaceModel> Spaces { get; }

	/// <summary>
	/// Not observalbe.  Will never change unless Space collection changes.  
	/// Watch Spaces to trigger for changes in WorldBounds
	/// </summary>
	public Bounds WorldBounds { get; set; }

	#region constructor

	public IslandModel(GameState gameState, OptionViewManager ovm) {
		ActionScope.ThrowIfMissingCurrent();

		_tokens = gameState.Tokens;
		_ovm = ovm;

		Spaces = [.. gameState.SpaceSpecs_Unfiltered.Select(BuildSpaceModel)];
		WorldBounds = CalcWorldBounds();

		gameState.NewLogEntry += GameState_NewLogEntry;
	}

	SpaceModel BuildSpaceModel(SpaceSpec spec) => new SpaceModel(_tokens[spec], spec.Layout, _ovm);
	Bounds CalcWorldBounds() => new BoundsBuilder(Spaces.Select(x => x.Bounds)).GetBounds();
	#endregion constructor

	/// <summary> Called when a new descision is available to Sync the view model to the GameState </summary>
	public void Sync() {
		ActionScope.ThrowIfMissingCurrent();
		foreach (SpaceModel spaceModel in Spaces )
			spaceModel.Sync();
	}

	/// <summary> Cleanup / Shutdown / Dispose </summary>
	public void ClearIslandTokens() {
		// !!! Make Island respond PROPERLY to spaces being added and removed,
		// then we don't have explicitly clear the tokens
		foreach (SpaceModel spaceModel in Spaces )
			spaceModel.ClearTokens();
		Spaces.Clear();
	}

	#region private methods

	void GameState_NewLogEntry(Log.ILogEntry obj) {
		if( obj is Log.LayoutChanged layoutChange ) UpdateLayout();
	}

	void UpdateLayout() {

		// Sync spaces
		var spacesToAdd = GameState.Current.SpaceSpecs_Unfiltered.ToHashSet();

		// Set new World Bounds BEFORE we update the Space collection.
		WorldBounds = new BoundsBuilder( spacesToAdd.Select(x=>x.Layout.Bounds)).GetBounds();

		// Compare Old to New (and remove any that are old)
		foreach( var current in Spaces.ToArray() ) {
			if( spacesToAdd.Contains(current.Space.SpaceSpec) )
				spacesToAdd.Remove(current.Space.SpaceSpec);
			else
				Spaces.Remove(current);
		}
		// Add which are actually new
		foreach( var newSpaceSpec in spacesToAdd )
			Spaces.Add(BuildSpaceModel(newSpaceSpec));
	}

	#endregion private methods

	#region private fields
	readonly Tokens_ForIsland _tokens = new Tokens_ForIsland();
	readonly OptionViewManager _ovm;
	#endregion
}
