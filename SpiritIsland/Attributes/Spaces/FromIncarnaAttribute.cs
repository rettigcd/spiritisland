namespace SpiritIsland;

[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
public class FromIncarnaAttribute : TargetSpaceAttribute {
	public FromIncarnaAttribute()
		: base( TargetFrom.Incarna, 0, Filter.Incarna ) { }
	public override string RangeText => $"-";
}