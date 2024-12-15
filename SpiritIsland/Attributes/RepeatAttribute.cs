namespace SpiritIsland;

[AttributeUsage(AttributeTargets.Method|AttributeTargets.Class)]
public abstract class RepeatAttribute : Attribute, IHaveARepeater {

	/// <summary>
	/// Displays Repeats as Element-Threshold Tiers.
	/// </summary>
	public abstract IDrawableInnateTier[] ThresholdTiers { get; }

	public abstract IPowerRepeater GetRepeater();
}

public interface IHaveARepeater {
	public abstract IPowerRepeater GetRepeater();
}