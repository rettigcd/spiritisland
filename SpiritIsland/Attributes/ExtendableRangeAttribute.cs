namespace SpiritIsland;

/// <summary>
/// Targets a space, but conditionally has an extended range if certain Elemental thresholds are reached
/// </summary>
[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
public class ExtendableRangeAttribute : TargetSpaceAttribute {

	readonly string _triggeringElements;
	readonly int _extension;

	public ExtendableRangeAttribute( TargetFrom from, int range, string triggeringElements, int extention, params string[] targetType ) 
		: base( from, range, targetType ) 
	{
		_triggeringElements = triggeringElements;
		_extension = extention;
	}

	public override string RangeText => "+"+_extension;

	protected override async Task<int> CalcRange( SelfCtx ctx ) => _range
		+ (await ctx.YouHave( _triggeringElements ) ? _extension : 0);

	public override LandOrSpirit LandOrSpirit => LandOrSpirit.Land;

}
