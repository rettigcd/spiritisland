namespace SpiritIsland;

[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
public class FromPresenceInAttribute : TargetSpaceAttribute {
	public FromPresenceInAttribute( int range, Terrain sourceTerrain, string filter = Target.Any )
		: base( 
			new TargetingSourceCriteria( From.Presence, sourceTerrain), 
			range, filter 
		) {}
	public override string RangeText => $"{_range}:{_sourceCriteria.Terrain}";
}
