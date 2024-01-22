namespace SpiritIsland;

public class GrowthTrack {

	#region constructors

	public GrowthTrack( params GrowthOption[] options ) {
		groups.Add( new GrowthPickGroups(options));
	}

	public GrowthTrack( int pick, params GrowthOption[] options ) {
		groups.Add( new GrowthPickGroups(pick,options));
	}

	public GrowthTrack Add( GrowthPickGroups grp ) {
		groups.Add(grp);
		return this;
	}

	#endregion

	public ReadOnlyCollection<GrowthPickGroups> PickGroups => groups.AsReadOnly();

	public GrowthOption[] Options => groups.SelectMany(g=>g.Options).ToArray();

	public virtual IGrowthPhaseInstance GetInstance() => new GrowthPhaseInstance( groups );

	#region private

	readonly List<GrowthPickGroups> groups = [];

	#endregion

}
