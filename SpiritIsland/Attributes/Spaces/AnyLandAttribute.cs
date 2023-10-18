namespace SpiritIsland;

[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
public class AnyLandAttribute : TargetSpaceAttribute {
	public AnyLandAttribute( params string[] filters )
		: base( 
			new TargetingSourceCriteria( From.Presence ), 
			100, filters 
		) { }
	public override string RangeText => "-";
}
