namespace SpiritIsland.An;

public class Element : A.TypedDecision<ElementOption> {
	public Element(string prompt, IEnumerable<SpiritIsland.Element> elementOptions, Present present, SpiritIsland.Element focusElement = SpiritIsland.Element.None)
		:base(prompt, elementOptions.Select( x=>new ElementOption( x, x==focusElement ) ), present ) { 
		ElementOptions = elementOptions.Select( x => new ElementOption( x, x==focusElement ) ).ToArray();
	}

	public ElementOption[] ElementOptions { get; }
}

public record ElementOption : ItemOption<SpiritIsland.Element> {
	public ElementOption( SpiritIsland.Element element, bool isFocus ) : base( element ) { IsFocus = isFocus; }
	public bool IsFocus { get; }
}