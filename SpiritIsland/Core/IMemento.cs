namespace SpiritIsland;

/// <summary>
/// Used to save/restore internal state;
/// </summary>
public interface IHaveMemento {
	object Memento { get; set; }

}

public static class MementoExtenstions {
	static public void Save( this Dictionary<IHaveMemento, object> mementos, IHaveMemento holder ) { 
		if(holder is not null) mementos[holder] = holder.Memento;
	}
	static public void SaveMany( this Dictionary<IHaveMemento, object> mementos, IEnumerable items ) { 
		foreach(var item in items.OfType<IHaveMemento>())
			mementos.Save( item );
	}
	static public void Restore( this Dictionary<IHaveMemento, object> mementos ) {
		foreach(var pair in mementos)
			pair.Key.Memento = pair.Value;
	}
}