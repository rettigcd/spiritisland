namespace SpiritIsland;

public class GrowthTrack {

	#region constructors

	public GrowthTrack( params GrowthGroup[] groups ) {
		this.groups.Add( new GrowthPickGroups( groups ) );
	}

	public GrowthTrack( int pick, params GrowthGroup[] groups ) {
		this.groups.Add( new GrowthPickGroups( pick, groups ) );
	}

	public GrowthTrack Add( GrowthPickGroups grp ) {
		groups.Add(grp);
		return this;
	}

	#endregion

	public ReadOnlyCollection<GrowthPickGroups> PickGroups => groups.AsReadOnly();

	public GrowthGroup[] Groups => groups.SelectMany(g=>g.Groups).ToArray();

	public IEnumerable<IActOn<Spirit>> GrowthActions => Groups.SelectMany(g=>g.Actions);

	public virtual IGrowthPhaseInstance GetInstance() => new GrowthPhaseInstance( groups );

	#region private

	readonly List<GrowthPickGroups> groups = [];

	#endregion

}
