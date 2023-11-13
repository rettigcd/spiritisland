namespace SpiritIsland;
#nullable enable

public class AddDestroyedPresence : SpiritAction {

	// The alternate way to place destroyed presence
	// is through BoundPresence.PlaceDestroyedHere()

	#region constructors

	public AddDestroyedPresence( int range ):base(){
		Range = range;
	}

	#endregion

	public int Range { get; }

	public override string Description
		=> $"Add{CountStr} Destroyed Presence{RangeStr}";
	string RangeStr => Range == 0 ? "" : " - Range " + Range;
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

	public override async Task ActAsync( SelfCtx ctx ) {
		Spirit placingSpirit = ctx.Self;
		SpiritPresence presence = placingSpirit.Presence;
		if(presence.Destroyed == 0) return;

		int maxToPlaceOnSpace = Math.Min( presence.Destroyed, NumToPlace );

		var destinationOptions = (_relativeSpirit ?? placingSpirit)
			.FindSpacesWithinRange( new TargetCriteria( Range ) )
			.Where( placingSpirit.Presence.CanBePlacedOn )
			.Downgrade();

		Space dst = await placingSpirit.Select( A.Space.ToPlaceDestroyedPresence(
			destinationOptions,
			_present,
			placingSpirit,
			maxToPlaceOnSpace
		) );

		if(dst == null ) return;

		int numToPlace = _present == Present.Always ? maxToPlaceOnSpace
			: await placingSpirit.SelectNumber("How many presences would you like to place?", maxToPlaceOnSpace, 1);
		if(numToPlace == 0 ) return;

		await presence.PlaceDestroyedAsync(maxToPlaceOnSpace, dst);
		if( _placedCallback != null )
			await _placedCallback(maxToPlaceOnSpace,dst);
	}

	public int NumToPlace { get; private set; } = 1; // default to placing 1
	Present _present = Present.Always; // defaults to being required
	Spirit? _relativeSpirit = null;
	Func<int,Space,Task>? _placedCallback;
}