namespace SpiritIsland;

[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
public class FromPresenceInAttribute : TargetSpaceAttribute {

	public FromPresenceInAttribute( string restrictSource, int range, params string[] filters )
		: base(
			new TargetingSourceCriteria( From.Presence, restrictSource ),
			range, filters
		) { }


	public override string RangeText => $"{_range}:{_sourceCriteria.Restrict}";
}
