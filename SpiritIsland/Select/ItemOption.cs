namespace SpiritIsland;

public class ItemOption<T> : IOption {
	public T Item { get; }
	public ItemOption( T item ) { Item = item; }
	public string Text => Item.ToString();
}

public class ElementOption : ItemOption<Element> {
	public ElementOption( Element element ) : base( element ) { }
}