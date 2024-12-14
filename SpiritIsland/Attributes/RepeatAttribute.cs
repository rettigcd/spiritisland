namespace SpiritIsland;

[AttributeUsage(AttributeTargets.Method|AttributeTargets.Class)]
public abstract class RepeatAttribute : Attribute {

	/// <summary>
	/// Displays Repeats as Element-Threshold Tiers.
	/// </summary>
	public abstract IDrawableInnateTier[] ThresholdTiers { get; }

	public abstract IPowerRepeater GetRepeater();
}
