namespace SpiritIsland;

[AttributeUsage(AttributeTargets.Method|AttributeTargets.Class)]
public abstract class RepeatAttribute : Attribute {
	public abstract IDrawableInnateOption[] Thresholds { get; }

	public abstract IPowerRepeater GetRepeater();
}
