namespace SpiritIsland;

[AttributeUsage(AttributeTargets.Method|AttributeTargets.Class)]
public abstract class RepeatAttribute : Attribute {
	public abstract IDrawableInnateTier[] Thresholds { get; }

	public abstract IPowerRepeater GetRepeater();
}
