namespace SpiritIsland;
#nullable enable

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

	public override string Description
		=> $"Add{CountStr} Destroyed Presence{RangeStr}";
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
	public AddDestroyedPresence WhenPlacedTrigger( Func<int, SpaceSpec, Task> callback ) {
		_placedCallback = callback;
		return this;
	}

	#endregion

	IEnumerable<Space> SpacesFromSourceSpirit( Spirit sourceSpirit ) {
		return Range.HasValue
			? sourceSpirit.FindSpacesWithinRange(new TargetCriteria(Range.Value))
			: sourceSpirit.Presence.Lands;
	}

	public override async Task ActAsync( Spirit placingSpirit) {
		SpiritPresence presence = placingSpirit.Presence;
		if(presence.Destroyed.Count == 0) return;

		int maxToPlaceOnSpace = Math.Min( presence.Destroyed.Count, NumToPlace );

		var sourceSpirit = _relativeSpirit ?? placingSpirit;
		
		IEnumerable<Space> destinationOptions = SpacesFromSourceSpirit(_relativeSpirit ?? placingSpirit)
			.Where(placingSpirit.Presence.CanBePlacedOn);

		Space dst = await placingSpirit.SelectAsync( A.SpaceDecision.ToPlaceDestroyedPresence(
			destinationOptions,
			_present,
			placingSpirit,
			maxToPlaceOnSpace
		) );

		if(dst == null ) return;

		int numToPlace = _present == Present.Always ? maxToPlaceOnSpace
			: await placingSpirit.SelectNumber("How many presences would you like to place?", maxToPlaceOnSpace, 1);
		if(numToPlace == 0 ) return;

		await presence.Destroyed.MoveToAsync(dst);
		//await presence.PlaceDestroyedAsync(maxToPlaceOnSpace, dst);
		if( _placedCallback != null )
			await _placedCallback(maxToPlaceOnSpace,dst.SpaceSpec);
	}

	public int NumToPlace { get; private set; } = 1; // default to placing 1
	Present _present = Present.Always; // defaults to being required
	Spirit? _relativeSpirit = null;
	Func<int,SpaceSpec,Task>? _placedCallback;
}