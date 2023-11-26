namespace SpiritIsland;

[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
public class FromSacredSiteAttribute : TargetSpaceAttribute {
	public FromSacredSiteAttribute( int range, params string[] filters )
		: base( TargetFrom.SacredSite, range, filters ) { }

	/// <param name="filterFrom">null or comma-delimited Target. strings</param>
	public FromSacredSiteAttribute( string filterFrom, int range, params string[] filters )
		: base( TargetFrom.SacredSite, filterFrom, range, filters ) { }

	public override string RangeText => $"{_range}:ss";
}
