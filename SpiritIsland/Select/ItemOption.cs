namespace SpiritIsland;

public record ItemOption<T> : IOption where T:notnull {
	public T Item { get; }
	public ItemOption( T item ) { Item = item; }
	public string Text => Item.ToString()!;
}