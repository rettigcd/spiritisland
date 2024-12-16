namespace SpiritIsland;

[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
public class FromPresenceAttribute : TargetSpaceAttribute {

	public FromPresenceAttribute(int range)
		: base(TargetFrom.Presence, range, []) { }

	public FromPresenceAttribute( int range, string[] filters )
		: base( TargetFrom.Presence, range, filters ) {}

	public FromPresenceAttribute(int range, string filter)
		: base(TargetFrom.Presence, range, [filter]) { }

	/// <param name="filterFrom">null or comma-delimited Target filters</param>
	public FromPresenceAttribute( string filterFrom, int range, params string[] filters )
		: base( TargetFrom.Presence, filterFrom, range, filters	) { }

	public override string RangeText => _restrictFrom == null 
		? _range.ToString()
		: $"{_range}:{_restrictFrom}";
}