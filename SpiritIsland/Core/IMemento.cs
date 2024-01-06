namespace SpiritIsland;

public interface IMemento<T> {}

/// <summary>
/// Used to save/restore internal state;
/// </summary>
public interface IHaveMemento {
	object Memento { get; set; }

}