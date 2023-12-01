namespace SpiritIsland;

public abstract class GeneratesContextAttribute : Attribute {
	public abstract Task<object> GetTargetCtx( string powerName, Spirit self );

	public abstract string RangeText { get; }

	public abstract string TargetFilterName { get; }

	public abstract LandOrSpirit LandOrSpirit { get; }

}

