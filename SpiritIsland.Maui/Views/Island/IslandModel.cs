namespace SpiritIsland.Maui;

public class IslandModel : ObservableModel {

	public record SpacesChangedEventArgs(SpaceModel[] Added, SpaceModel[] Removed );

	public event Action<SpacesChangedEventArgs>? SpacesChanged;

	public List<SpaceModel> Spaces { get; }

	/// <summary>
	/// Not observalbe.  Will never change unless Space collection changes.  
	/// Watch SpacesAdded event to trigger for changes in WorldBounds
	/// </summary>
	public Bounds WorldBounds { get; private set; }

	public IEnumerable<XY> Corners => Spaces.SelectMany(x => x.Corners);

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
	public void SyncTokens() {
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
		if( obj is Log.LayoutChanged ) UpdateExistingSpaces();
	}

	void UpdateExistingSpaces() {

		// Sync spaces
		var specsToAdd = GameState.Current.SpaceSpecs_Unfiltered.ToHashSet();
		var spacesRemoved = new List<SpaceModel>();

		// Set new World Bounds BEFORE we update the Space collection.
		WorldBounds = new BoundsBuilder( specsToAdd.Select(x=>x.Layout.Bounds)).GetBounds();

		// Compare Old to New (and remove any that are old)
		foreach( var current in Spaces.ToArray() ) {
			if( specsToAdd.Contains(current.Space.SpaceSpec) )
				specsToAdd.Remove(current.Space.SpaceSpec);
			else {
				current.ClearTokens();
				Spaces.Remove(current);
				spacesRemoved.Add(current);
			}
		}
		// Add which are actually new
		var spacesAdded = specsToAdd.Select(BuildSpaceModel).ToArray();
		Spaces.AddRange(spacesAdded);
		
		SpacesChanged?.Invoke(new SpacesChangedEventArgs(spacesAdded, [.. spacesRemoved]));
	}

	#endregion private methods

	#region private fields
	readonly Tokens_ForIsland _tokens = new Tokens_ForIsland();
	readonly OptionViewManager _ovm;
	#endregion
}
