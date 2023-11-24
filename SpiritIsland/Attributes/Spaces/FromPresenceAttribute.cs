namespace SpiritIsland;

[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
public class FromPresenceAttribute : TargetSpaceAttribute {
	public FromPresenceAttribute( int range, params string[] filters )
		: base( 
			new TargetingSourceCriteria( From.Presence ), 
			range, filters 
		) {}
	public FromPresenceAttribute( string restrictSource, int range, params string[] filters )
		: base(
			new TargetingSourceCriteria( From.Presence ) { Restrict = restrictSource },
			range, filters
		) { }

	public override string RangeText => _range.ToString();
}