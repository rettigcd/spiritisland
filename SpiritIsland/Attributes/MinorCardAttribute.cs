namespace SpiritIsland;

[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
public class MinorCardAttribute : CardAttribute {

	public MinorCardAttribute(string name, int cost, params Element[] elements)
		:base(name,cost,PowerType.Minor,elements)
	{ }

	public MinorCardAttribute( string name, int cost, string elementString )
		: base( name, cost, PowerType.Minor, ElementCounts.Parse(elementString) ) { }

}
