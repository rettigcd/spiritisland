namespace SpiritIsland.An;

public class Element( string prompt, IEnumerable<SpiritIsland.Element> elementOptions, Present present, SpiritIsland.Element focusElement = SpiritIsland.Element.None ) 
	: A.TypedDecision<ElementOption>(prompt, elementOptions.Select( x=>new ElementOption( x, x==focusElement ) ), present )
{
	public ElementOption[] ElementOptions { get; } = elementOptions.Select( x => new ElementOption( x, x == focusElement ) ).ToArray();
}

public record ElementOption : ItemOption<SpiritIsland.Element> {
	public ElementOption( SpiritIsland.Element element, bool isFocus ) : base( element ) { IsFocus = isFocus; }
	public bool IsFocus { get; }
}