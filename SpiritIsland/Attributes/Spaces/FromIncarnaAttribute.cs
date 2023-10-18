namespace SpiritIsland;

[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
public class FromIncarnaAttribute : TargetSpaceAttribute {
	public FromIncarnaAttribute()
		: base( 
			new TargetingSourceCriteria( From.Incarna ), 
			0, Target.Incarna 
		) { }
	public override string RangeText => $"S+";
}