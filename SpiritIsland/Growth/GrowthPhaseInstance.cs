namespace SpiritIsland;

public interface IGrowthPhaseInstance {
	GrowthOption[] RemainingOptions( int energy );
	void MarkAsUsed( GrowthOption option );
}

class GrowthPhaseInstance( IEnumerable<GrowthPickGroups> _gogs ) : IGrowthPhaseInstance {


	/// <summary> Filter Options that require more energy than we have. </summary>
	public GrowthOption[] RemainingOptions(int energy)
		=> remaining
			.Where( g=>g.HasAdditionalCounts )
			.SelectMany( g=>g.AvailableOptions )
			.Where( o => 0 <= o.GainEnergy + energy )
			.ToArray();

	public void MarkAsUsed( GrowthOption option ) {
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
		public bool HasOption( GrowthOption option ) => AvailableOptions.Contains(option);

		public IEnumerable<GrowthOption> AvailableOptions => _grp.Options.Except( used );

		public bool HasAdditionalCounts => _grp.SelectCount > used.Count;

		public void MarkUsed( GrowthOption option ) => used.Add( option );

		readonly List<GrowthOption> used = [];
	}

}