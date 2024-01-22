namespace SpiritIsland;

/// <summary>
/// Targets a space, but conditionally has an extended range if certain Elemental thresholds are reached
/// </summary>
[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
public class ExtendableRangeAttribute( 
	TargetFrom from, 
	int range, 
	string triggeringElements, 
	int extention, 
	params string[] targetType 
) : TargetSpaceAttribute( from, range, targetType ) 
{
	readonly string _triggeringElements = triggeringElements;
	readonly int _extension = extention;

	public override string RangeText => "+"+_extension;

	protected override async Task<int> CalcRange( Spirit self ) => _range
		+ (await self.YouHave( _triggeringElements ) ? _extension : 0);

	public override LandOrSpirit LandOrSpirit => LandOrSpirit.Land;

}
