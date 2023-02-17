namespace SpiritIsland;

public class ActionScopeValue<T> {
	readonly Func<T> _initializer;
	readonly T _defaultValue;
	readonly string _key;

	/// <summary>
	/// Returns default value until it is manually initialized.
	/// </summary>
	public ActionScopeValue(string key, T defaultValue){
		_key = key;
		_defaultValue = defaultValue;
	}

	/// <summary>
	/// Auto-initializes it first time it is read.
	/// </summary>
	public ActionScopeValue( string key, Func<T> initializer ) {
		_key = key;
		_initializer = initializer;
	}

	public bool HasValue => ActionScope.Current.ContainsKey(_key);

	public T Value {
		get => _initializer != null 
			? ActionScope.Current.SafeGet<T>(_key,_initializer)
			: ActionScope.Current.SafeGet<T>(_key,_defaultValue);
		set => ActionScope.Current[_key] = value;
	}

}