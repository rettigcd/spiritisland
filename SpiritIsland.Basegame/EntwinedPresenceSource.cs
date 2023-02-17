namespace SpiritIsland.Basegame;

class EntwinedPresenceSource : ICalcPowerTargetingSource {

	readonly Spirit[] spirits;
	readonly ICalcPowerTargetingSource[] origApis;

	public EntwinedPresenceSource( params Spirit[] spirits ) {
		this.spirits = spirits;
		this.origApis = spirits.Select(x=>x.TargetingSourceCalc).ToArray();
			
		foreach(var spirit in spirits)
			spirit.TargetingSourceCalc = this;
	}

	public IEnumerable<SpaceState> FindSources( IKnowSpiritLocations presence, TargetingSourceCriteria sourceCriteria ) {
		List<SpaceState> sources = new();
		// Find source of original
		for(int i = 0; i<spirits.Length; ++i)
			sources.AddRange(origApis[i].FindSources( spirits[i].Presence, sourceCriteria ) );
		return sources.Distinct();
	}
}