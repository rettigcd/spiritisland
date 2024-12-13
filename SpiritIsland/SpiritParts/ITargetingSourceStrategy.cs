namespace SpiritIsland;

public interface ITargetingSourceStrategy {
	IEnumerable<Space> EvaluateFrom( IKnowSpiritLocations presence, TargetFrom from );
}

