namespace SpiritIsland;

public class GrowthTrack {

	#region constructors

	public GrowthTrack( params GrowthGroup[] groups ) {
		this._pickGroups.Add( new GrowthPickGroups( groups ) );
	}

	public GrowthTrack( int pick, params GrowthGroup[] groups ) {
		this._pickGroups.Add( new GrowthPickGroups( pick, groups ) );
	}

	public GrowthTrack Add( GrowthPickGroups grp ) {
		_pickGroups.Add(grp);
		return this;
	}

	#endregion

	public ReadOnlyCollection<GrowthPickGroups> PickGroups => _pickGroups.AsReadOnly();

	public GrowthGroup[] Groups => _pickGroups.SelectMany(g=>g.Groups).ToArray();

	public IEnumerable<IActOn<Spirit>> GrowthActions => Groups.SelectMany(g=>g.Actions);

	public virtual IGrowthPhaseInstance GetInstance() => new GrowthPhaseInstance( _pickGroups );

	#region private

	readonly List<GrowthPickGroups> _pickGroups = [];

	#endregion

}
