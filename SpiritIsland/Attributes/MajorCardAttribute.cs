namespace SpiritIsland;

[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
public class MajorCardAttribute : CardAttribute {
	public MajorCardAttribute(string name, int cost, params Element[] elements)
		:base(name,cost,PowerType.Major,new CountDictionary<Element>(elements))
	{ }
	public MajorCardAttribute(string name, int cost, string elements)
		:base(name,cost,PowerType.Major,ElementStrings.Parse(elements))
	{ }

}
