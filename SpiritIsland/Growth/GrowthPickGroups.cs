namespace SpiritIsland;

/// <summary> 
/// Configures how how GrowthActionGroups may be selected
/// </summary>
/// <remarks>
/// GrowhTrack > GrowthPickGroups > GrowthActionGroup
/// Most spirits have 1 of these with SelectCount = 1 (Pick 1) or 2 (Pick 2)
/// Lure has 2 of these, each containing 2 GrowthOptions, each with SelectCount = 1
/// Rampant Green has 2, 1 for 1st Growth Option and 1 for the last 3.
/// </remarks>
public class GrowthPickGroups {
	// !! Merge this Grouping Functionality up into the GrowthTrack using GroupIds instead of these pick groups.

	#region constructor
	/// <summary> Allows user to select N of the Action Groups// </summary>
	public GrowthPickGroups( int selectCount, params GrowthOption[] options ) {
		SelectCount = selectCount;
		Options = options;
	}
	/// <summary> Allows user to select 1 of the Action Groups// </summary>
	public GrowthPickGroups( params GrowthOption[] options ) {
		SelectCount = 1;
		Options = options;
	}
	#endregion

	public int SelectCount { get; }

	public GrowthOption[] Options { get; private set; }

	public void Add( GrowthOption option ) { // hook for Starlight
		var options = Options.ToList();
		options.Add(option);
		Options = options.ToArray();
	}

}
