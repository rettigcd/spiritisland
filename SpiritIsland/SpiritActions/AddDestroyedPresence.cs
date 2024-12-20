namespace SpiritIsland;

/// <remarks>
/// Used by Powers + Growth for Wounded Waters and Relentless Gaze
/// </remarks>
public class AddDestroyedPresence : SpiritAction {

	#region constructors

	/// <summary> Destroy presence must be within a certain range. </summary>
	public AddDestroyedPresence(int range) {
		Range = range;
	}

	/// <summary> Destroy presence to a spirits lands. </summary>
	public AddDestroyedPresence() {
		Range = null;
	}

	#endregion

	public int? Range { get; }

	public override string Description => $"Add{CountStr} Destroyed Presence{RangeStr}";
	string RangeStr => (Range == null || Range == 0) ? "" : " - Range " + Range;
	string CountStr => _present == Present.Done ? $" up to {NumToPlace}"
		: NumToPlace != 1 ? $" {NumToPlace}"
		: "";

	#region Config variants

	/// <summary>
	/// Changes # of presence to place on the same space from 1-required
	/// </summary>
	public AddDestroyedPresence SetNumToPlace(int maxToPlace, Present present ) {
		if(maxToPlace <= 0) throw new ArgumentOutOfRangeException(nameof(maxToPlace),"must place at least 1 presence");
		NumToPlace = maxToPlace;
		_present = present;
		return this;
	}

	/// <summary> Range using a different spirit. </summary>
	public AddDestroyedPresence RelativeTo( Spirit relativeSpirit ) {
		_relativeSpirit = relativeSpirit;
		return this;
	}

	/// <summary> Adds a callback to be called when/if destroyed presence is placed. </summary>
	public AddDestroyedPresence WhenPlacedTrigger( Func<int, Space, Task> callback ) {
		_placedCallback = callback;
		return this;
	}

	#endregion

	Space[] SpacesFromSourceSpirit( Spirit sourceSpirit ) {
		return sourceSpirit.FindSpacesWithinRange(new TargetCriteria(Range??0));
	}

	public override async Task ActAsync( Spirit placingSpirit) {
		SpiritPresence presence = placingSpirit.Presence;
		int maxToPlaceOnSpace = Math.Min(presence.Destroyed.Count, NumToPlace);
		if( maxToPlaceOnSpace == 0) return;

		// Must Place max if possible. (when used by Blazing Renewal)

		// destination
		var destinationOptions = SpacesFromSourceSpirit(_relativeSpirit ?? placingSpirit)
			.Where(placingSpirit.Presence.CanBePlacedOn)
			.ToArray();
		if(destinationOptions.Length == 0) return;

		var move = await placingSpirit.SelectAlways($"Place {maxToPlaceOnSpace} Destroyed Presence", presence.Destroyed.BuildMoves(destinationOptions) );

		await move.Apply(maxToPlaceOnSpace);

		if( _placedCallback is not null && move.Destination is Space dstSpace )
			await _placedCallback(maxToPlaceOnSpace, dstSpace);
	}

	public int NumToPlace { get; private set; } = 1; // default to placing 1
	Present _present = Present.Always; // defaults to being required
	Spirit? _relativeSpirit = null;
	Func<int,Space,Task>? _placedCallback;
}