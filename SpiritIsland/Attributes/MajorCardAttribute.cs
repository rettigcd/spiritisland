namespace SpiritIsland;

[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
public class MajorCardAttribute : CardAttribute {
	public MajorCardAttribute(string name, int cost, params Element[] elements)
		:base(name,cost,PowerType.Major,elements)
	{ }
}
