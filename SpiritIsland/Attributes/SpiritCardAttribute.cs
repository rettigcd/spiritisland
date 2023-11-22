namespace SpiritIsland;

[AttributeUsage(AttributeTargets.Method)]
public class SpiritCardAttribute : CardAttribute {

	public SpiritCardAttribute(string name, int cost, params Element[] elements)
		:base(name,cost,PowerType.Spirit,new CountDictionary<Element>(elements))
	{ }
	public SpiritCardAttribute(string name, int cost, string elementString)
		:base(name,cost,PowerType.Spirit,ElementStrings.Parse(elementString))
	{ }

}