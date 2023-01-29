namespace SpiritIsland.Select;

public class Element : TypedDecision<ElementOption> {
	public Element(string prompt, IEnumerable<SpiritIsland.Element> elementOptions, Present present)
		:base(prompt, elementOptions.Select( x=>new ElementOption( x ) ), present ) { 
		ElementOptions = elementOptions.Select( x => new ElementOption( x ) ).ToArray();
	}

	public ElementOption[] ElementOptions { get; }
}