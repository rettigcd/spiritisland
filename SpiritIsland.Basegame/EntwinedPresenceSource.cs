namespace SpiritIsland.Basegame;

class EntwinedPresenceSource : ICalcSource {

	readonly Spirit[] spirits;
	readonly ICalcSource[] origApis;

	public EntwinedPresenceSource( params Spirit[] spirits ) {
		this.spirits = spirits;
		this.origApis = spirits.Select(x=>x.SourceCalc).ToArray();
			
		foreach(var spirit in spirits)
			spirit.SourceCalc = this;
	}

	public IEnumerable<SpaceState> FindSources( GameState gs, IKnowSpiritLocations presence, TargetSourceCriteria sourceCriteria, TerrainMapper mapper ) {
		List<SpaceState> sources = new();
		// Find source of original
		for(int i = 0; i<spirits.Length; ++i)
			sources.AddRange(origApis[i].FindSources( gs, spirits[i].BindMyPower(gs).Presence, sourceCriteria, mapper ) );
		return sources.Distinct();
	}
}