namespace SpiritIsland;

public abstract class GeneratesContextAttribute : Attribute, ITargetCtxFactory {

	public abstract Task<object> GetTargetCtx( string powerName, Spirit self );

	public abstract string RangeText { get; }

	public abstract string TargetFilterName { get; }

	public abstract LandOrSpirit LandOrSpirit { get; }

}

public interface ITargetCtxFactory {
	Task<object> GetTargetCtx(string powerName, Spirit self);
	string RangeText { get; }
	string TargetFilterName { get; }
	LandOrSpirit LandOrSpirit { get; }
}
