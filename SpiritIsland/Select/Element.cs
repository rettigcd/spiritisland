namespace SpiritIsland.An;

public class Element : A.TypedDecision<ElementOption> {
	public Element(string prompt, IEnumerable<SpiritIsland.Element> elementOptions, Present present)
		:base(prompt, elementOptions.Select( x=>new ElementOption( x ) ), present ) { 
		ElementOptions = elementOptions.Select( x => new ElementOption( x ) ).ToArray();
	}

	public ElementOption[] ElementOptions { get; }
}

public record ElementOption : ItemOption<SpiritIsland.Element> {
	public ElementOption( SpiritIsland.Element element ) : base( element ) { }
}