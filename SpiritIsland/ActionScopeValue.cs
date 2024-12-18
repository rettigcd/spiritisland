#nullable enable
namespace SpiritIsland;

/// <summary>
/// Auto-initializes it first time it is read.
/// </summary>
public class ActionScopeValue<T>(string key, Func<T> initializer) {
	public bool HasValue => ActionScope.Current.ContainsKey(key);

	public T Value {
		get => ActionScope.Current.SafeGet(key,initializer);
		set => ActionScope.Current.SafeSet(key,value);
	}

}

/// <summary> Holds nullable types </summary>
/// <remarks> Returns null until it is manually initialized. </remarks>
public class ActionScopeValueNullable<T>(string key) where T:class {
	public bool HasValue => ActionScope.Current.ContainsKey(key);

	public T? Value {
		get => ActionScope.Current.SafeGetNullable<T>(key);
		set => ActionScope.Current.SafeSetNullable(key,value);
	}

}