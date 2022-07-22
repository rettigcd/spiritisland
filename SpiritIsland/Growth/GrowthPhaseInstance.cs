namespace SpiritIsland;

public interface IGrowthPhaseInstance {
	GrowthOption[] RemainingOptions( int energy );
	void MarkAsUsed( GrowthOption option );
}

class GrowthPhaseInstance : IGrowthPhaseInstance {

	public GrowthPhaseInstance( IEnumerable<GrowthPickGroups> gogs ) {
		remaining = gogs
			.Select(g=> new GOGRemaining( g ) )
			.ToArray();
	}

	

	public GrowthOption[] RemainingOptions(int energy)
		=> remaining
			.Where( g=>g.HasAdditionalCounts )
			.SelectMany( g=>g.AvailableOptions )
			.Where( o => o.GainEnergy + energy >= 0 )
			.ToArray();

	public void MarkAsUsed( GrowthOption option ) {
		var grp = remaining.First(grp=>grp.HasOption(option));
		grp.MarkUsed( option );
	}

	#region private

	// Current status of executing growth options
	readonly GOGRemaining[] remaining;

	#endregion

	class GOGRemaining {

		public GOGRemaining( GrowthPickGroups grp ) { this.grp = grp; }

		public bool HasOption( GrowthOption option ) => AvailableOptions.Contains(option);

		public IEnumerable<GrowthOption> AvailableOptions => grp.Options.Except( used );

		public bool HasAdditionalCounts => grp.SelectCount > used.Count;

		public void MarkUsed( GrowthOption option ) => used.Add( option );

		readonly List<GrowthOption> used = new List<GrowthOption>();
		readonly GrowthPickGroups grp;
	}

}