namespace SpiritIsland;

public interface IGrowthPhaseInstance {
	GrowthGroup[] RemainingOptions( int energy );
	void MarkAsUsed( GrowthGroup option );
}

class GrowthPhaseInstance( IEnumerable<GrowthPickGroups> _gogs ) : IGrowthPhaseInstance {


	/// <summary> Filter Options that require more energy than we have. </summary>
	public GrowthGroup[] RemainingOptions(int energy)
		=> remaining
			.Where( g=>g.HasAdditionalCounts )
			.SelectMany( g=>g.AvailableOptions )
			.Where( o => 0 <= o.GainEnergy + energy )
			.ToArray();

	public void MarkAsUsed( GrowthGroup option ) {
		var grp = remaining.First(grp=>grp.HasOption(option));
		grp.MarkUsed( option );
	}

	#region private

	// Current status of executing growth options
	readonly GOGRemaining[] remaining = _gogs
			.Select( g => new GOGRemaining( g ) )
			.ToArray();

	#endregion

	class GOGRemaining( GrowthPickGroups _grp ) {
		public bool HasOption( GrowthGroup group ) => AvailableOptions.Contains(group);

		public IEnumerable<GrowthGroup> AvailableOptions => _grp.Groups.Except( _used );

		public bool HasAdditionalCounts => _grp.SelectCount > _used.Count;

		public void MarkUsed( GrowthGroup group ) => _used.Add( group );

		readonly List<GrowthGroup> _used = [];
	}

}