namespace SpiritIsland.Basegame;

class EntwinedPresenceSource : ICalcPowerSource {

	readonly Spirit[] spirits;
	readonly ICalcPowerSource[] origApis;

	public EntwinedPresenceSource( params Spirit[] spirits ) {
		this.spirits = spirits;
		this.origApis = spirits.Select(x=>x.SourceCalc).ToArray();
			
		foreach(var spirit in spirits)
			spirit.SourceCalc = this;
	}

	public IEnumerable<SpaceState> FindSources( IKnowSpiritLocations presence, TargetSourceCriteria sourceCriteria, GameState gs ) {
		List<SpaceState> sources = new();
		// Find source of original
		for(int i = 0; i<spirits.Length; ++i)
			sources.AddRange(origApis[i].FindSources( new ReadOnlyBoundPresence( spirits[i], gs ), sourceCriteria, gs ) );
		return sources.Distinct();
	}
}