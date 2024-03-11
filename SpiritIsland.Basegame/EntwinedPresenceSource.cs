namespace SpiritIsland.Basegame;

class EntwinedPresenceSource : ITargetingSourceStrategy {

	readonly Dictionary<SpiritPresence, ITargetingSourceStrategy> _olds;

	public EntwinedPresenceSource( params Spirit[] spirits ) {
		_olds = spirits.ToDictionary(s=>s.Presence,s=>s.TargetingSourceStrategy);

		foreach(Spirit spirit in spirits)
			spirit.TargetingSourceStrategy = this;
	}

	public IEnumerable<Space> EvaluateFrom( IKnowSpiritLocations presence, TargetFrom from ) {
		return _olds
			.SelectMany( p=> p.Value.EvaluateFrom(p.Key, from) )
			.Distinct();
	}

}