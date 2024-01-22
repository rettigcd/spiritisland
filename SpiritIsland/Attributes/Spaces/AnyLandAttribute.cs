namespace SpiritIsland;

[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
public class AnyLandAttribute( params string[] filters ) 
	: TargetSpaceAttribute( TargetFrom.Presence, 100, filters )
{
	public override string RangeText => "-";
}
