namespace SpiritIsland;

/// <summary> 
/// Combines:
/// 	- Collection of GrowthGroups 
/// 	- Number of GrowthGroups a user must pick.
/// </summary>
/// <remarks>
/// GrowhTrack > GrowthPickGroups > GrowthActionGroup
/// Most spirits have 1 of these with SelectCount = 1 (Pick 1) or 2 (Pick 2)
/// Lure has 2 of these, each containing 2 GrowthOptions, each with SelectCount = 1
/// Rampant Green has 2, 1 for 1st Growth Option and 1 for the last 3.
/// </remarks>
/// <remarks> Allows user to select N of the Action Groups// </remarks>
public class PickGroups(int selectCount, params GrowthGroup[] groups) {

	public int MustPickCount => selectCount;

	public GrowthGroup[] Groups { get; private set; } = groups;

	public void Add( GrowthGroup group ) { // hook for Starlight
		Groups = [..Groups, group];
	}

	// --------  Instance Tracking  ---------
	public void Reset() {
		foreach( var group in Groups )
			group.Used = false;
	}

	public bool HasOption( GrowthGroup group ) => AvailableOptions.Contains(group);

	public IEnumerable<GrowthGroup> AvailableOptions => Groups.Where( g => !g.Used );

	public bool HasAdditionalCounts => Groups.Count( g => g.Used ) < MustPickCount;
	public void MarkUsed( GrowthGroup group ) => group.Used = true;

}
