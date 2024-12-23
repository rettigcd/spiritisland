namespace SpiritIsland;

/// <summary>
/// Holds mods that implement various interfaces.
/// Caches implemented types in arrays to avoid Reflection on every call.
/// </summary>
public class ModBucket {

	public void Add(ISpiritMod mod) { 
		_mods.Add(mod);
		_typeArrayLookup.Clear(); // invalidate/recalculate
	}

	public void Remove(ISpiritMod mod) {
		_mods.Remove(mod);
		_typeArrayLookup.Clear(); // invalidate/recalculate
	}

	/// <summary>
	/// Caches each type so we don't have to reflect on each type every time we call it.
	/// </summary>
	public IEnumerable<T> OfType<T>() {
		var type = typeof(T);
		if( _typeArrayLookup.ContainsKey(type) ) return (T[])_typeArrayLookup[type];
		var result = _mods.OfType<T>().ToArray();
		_typeArrayLookup.Add(type,result);
		return result;
	}

	#region private fields

	Dictionary<Type, object> _typeArrayLookup = [];

	List<ISpiritMod> _mods = [];

	#endregion private fields

}