namespace SpiritIsland;

// Base Attribute type for Major / Minor / Spirit cards 
[AttributeUsage(AttributeTargets.Method)]
public class CardAttribute : Attribute, IDisplayCardDetails {

	protected CardAttribute(string name, int cost, PowerType type, CountDictionary<Element> elements){
		Name = name;
		Cost = cost;
		PowerType = type;
		Elements = elements;
	}

	public string Name { get; }
	public int Cost { get; }
	public CountDictionary<Element> Elements { get; }
	public PowerType PowerType { get; }

}

public interface IDisplayCardDetails {
	public string Name { get; }
	public int Cost { get; }
	public CountDictionary<Element> Elements { get; }
	public PowerType PowerType { get; }
}