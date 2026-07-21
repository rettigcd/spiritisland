namespace SpiritIsland;

public class GrowthTrack {

	#region constructors

	public GrowthTrack( params GrowthGroup[] groups ) {
		this._pickGroups.Add( new PickGroups( 1, groups ) );
	}

	public GrowthTrack( int pick, params GrowthGroup[] groups ) {
		this._pickGroups.Add( new PickGroups( pick, groups ) );
	}

	public GrowthTrack Add( PickGroups grp ) {
		_pickGroups.Add(grp);
		return this;
	}

	#endregion

	public ReadOnlyCollection<PickGroups> PickGroups => _pickGroups.AsReadOnly();

	public GrowthGroup[] Groups => _pickGroups.SelectMany(g=>g.Groups).ToArray();

	public IEnumerable<IActOn<Spirit>> GrowthActions => Groups.SelectMany(g=>g.Actions);

	#region Growth - Instance tracking

	public void Reset() {
		foreach(var pickGroup in _pickGroups)
			pickGroup.Reset();
	}

	/// <summary> Filter Options that require more energy than we have. </summary>
	public GrowthGroup[] RemainingOptions(int energy)
		=> _pickGroups
			.Where( g=>g.HasAdditionalCounts )
			.SelectMany( g=>g.AvailableOptions )
			.Where( o => 0 <= o.GainEnergy + energy )
			.ToArray();

	/// <summary> Finds the Pick-Group with the GrowthGroup and marks it used. </summary>
	public void MarkAsUsed( GrowthGroup option ) {
		var grp = _pickGroups.First(grp=>grp.HasOption(option));
		grp.MarkUsed( option );
	}

	#endregion Growth - Instance tracking

	#region private

	readonly List<PickGroups> _pickGroups = [];

	#endregion

}
