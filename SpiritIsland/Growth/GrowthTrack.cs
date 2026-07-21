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

	#region Json

	/// <summary>
	/// Only which GrowthGroups are Used this round - [p,g] position pairs, same identity scheme
	/// Spirit.SerializeGrowthAction already uses (PickGroups[p].Groups[g]). The Groups/PickGroups
	/// structure itself is still spirit-type data, identical every time this Spirit's aspect setup is
	/// replayed - not captured here (see Spirit.ToJson's remarks).
	/// </summary>
	public JsonArray ToJson( ISerializationContext ctx ) => new JsonArray(
		_pickGroups.SelectMany( (pg, p) => pg.Groups
			.Select( (g, gi) => (p, gi, g) )
			.Where( t => t.g.Used )
			.Select( t => (JsonNode)new JsonArray( t.p, t.gi ) ) )
		.ToArray() );

	/// <summary> Restores onto an already-(re)constructed GrowthTrack - see Spirit.RestoreFromJson. </summary>
	public void RestoreFromJson( JsonArray json, ISerializationContext ctx ) {
		foreach( var pg in _pickGroups )
			foreach( var g in pg.Groups )
				g.Used = false;
		foreach( JsonNode? n in json ) {
			var pair = (JsonArray)n!;
			_pickGroups[ pair[0]!.GetValue<int>() ].Groups[ pair[1]!.GetValue<int>() ].Used = true;
		}
	}

	#endregion Json

	#region private

	readonly List<PickGroups> _pickGroups = [];

	#endregion

}
