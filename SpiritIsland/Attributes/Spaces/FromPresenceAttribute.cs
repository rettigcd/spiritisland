namespace SpiritIsland;

[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
public class FromPresenceAttribute : TargetSpaceAttribute {

	public FromPresenceAttribute( int range, params string[] filters )
		: base( TargetFrom.Presence, range, filters ) {}

	/// <param name="filterFrom">null or comma-delimited Target filters</param>
	public FromPresenceAttribute( string filterFrom, int range, params string[] filters )
		: base( TargetFrom.Presence, filterFrom, range, filters	) { }

	public override string RangeText => _restrictFrom == null 
		? _range.ToString()
		: $"{_range}:{_restrictFrom}";
}