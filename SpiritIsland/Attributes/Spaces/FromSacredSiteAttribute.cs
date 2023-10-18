namespace SpiritIsland;

[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
public class FromSacredSiteAttribute : TargetSpaceAttribute {
	public FromSacredSiteAttribute( int range, params string[] filters )
		: base( 
			new TargetingSourceCriteria( From.SacredSite ), 
			range, filters 
		) { }

	public FromSacredSiteAttribute( string restrictSource, int range, params string[] filters )
		: base(
			new TargetingSourceCriteria( From.SacredSite ) { Restrict = restrictSource },
			range, filters
		) { }


	public override string RangeText => $"{_range}:ss";
}
