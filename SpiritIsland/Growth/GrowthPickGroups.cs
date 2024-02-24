namespace SpiritIsland;

/// <summary> 
/// Configures how GrowthActionGroups may be selected
/// </summary>
/// <remarks>
/// GrowhTrack > GrowthPickGroups > GrowthActionGroup
/// Most spirits have 1 of these with SelectCount = 1 (Pick 1) or 2 (Pick 2)
/// Lure has 2 of these, each containing 2 GrowthOptions, each with SelectCount = 1
/// Rampant Green has 2, 1 for 1st Growth Option and 1 for the last 3.
/// </remarks>
public class GrowthPickGroups {

	#region constructor
	/// <summary> Allows user to select N of the Action Groups// </summary>
	public GrowthPickGroups( int selectCount, params GrowthGroup[] groups ) {
		SelectCount = selectCount;
		Groups = groups;
	}
	/// <summary> Allows user to select 1 of the Action Groups// </summary>
	public GrowthPickGroups( params GrowthGroup[] groups ) {
		SelectCount = 1;
		Groups = groups;
	}
	#endregion

	public int SelectCount { get; }

	public GrowthGroup[] Groups { get; private set; }

	public void Add( GrowthGroup group ) { // hook for Starlight
		Groups = [..Groups, group];
	}

}
