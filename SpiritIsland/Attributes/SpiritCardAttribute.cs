namespace SpiritIsland;

[AttributeUsage(AttributeTargets.Method)]
public class SpiritCardAttribute : CardAttribute {

	public SpiritCardAttribute(string name, int cost, params Element[] elements)
		:base(name,cost,PowerType.Spirit,elements)
	{ }

}