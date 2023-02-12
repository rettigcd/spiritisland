
namespace SpiritIsland;

/// <summary>
/// A Spirit Island 'Action'
/// </summary>
public sealed class ActionScope : IAsyncDisposable {

	// https://learn.microsoft.com/en-us/dotnet/api/system.threading.asynclocal-1?view=net-7.0
	// https://nelsonparente.medium.com/a-little-riddle-with-asynclocal-1fd11322067f
	public static ActionScope Current => _current.Value ?? throw new InvalidOperationException("Current action is null");
	public static ActionScope _TryGetCurrent => _current.Value; // gets current action if there is one, null otherwise
	readonly static AsyncLocal<ActionScope> _current = new AsyncLocal<ActionScope>(); // value gets shallow-copied into child calls and post-awaited states.

	#region constructor
	public ActionScope( ActionCategory actionCategory, TerrainMapper terrainMapper = null ) {
		Id = Guid.NewGuid();
		Category = actionCategory;

		_terrainMapper = terrainMapper;

		_old = _current.Value;
		_current.Value = this;
	}
	#endregion

	public readonly ActionCategory Category;
	public TerrainMapper TerrainMapper => _terrainMapper ??= GameState.Current.GetTerrain( Category );
	TerrainMapper _terrainMapper;

	readonly ActionScope _old;

	// spirit (if any) that owns the action. Null for non-spirit actions
	public Spirit Owner { get; set; }

	public Guid Id { get; }

	#region action-scoped data
	// String / object dictionary to track action things
	public bool ContainsKey(string key) => dict != null && dict.ContainsKey( key );

	public T SafeGet<T>(string key, T defaultValue=default) => ContainsKey(key) ? (T) this[key] : defaultValue;

	public T SafeGet<T>( string key, Func<T> initFunc ) {  
		if(ContainsKey( key )) return (T)this[key];
		T newValue = initFunc();
		this[key] = newValue;
		return newValue;
	}

	public object this[string key]{
		get => ContainsKey(key) ? dict[key] : throw new InvalidOperationException($"{key} was not set");
		set => (dict??= new())[key] = value;
	}
	#endregion

	public async ValueTask DisposeAsync() {
		if(_endOfThisAciton != null)
			await _endOfThisAciton.InvokeAsync(this);

		var current = _current.Value;
		if( current != this) 
			throw new Exception($"Disposing {Category}/{Id} but .Current is {current.Category}/{current.Id}");
		_current.Value = _old; // restore it
	}

	public void AtEndOfThisAction(Func<ActionScope,Task> action ) => (_endOfThisAciton ??= new AsyncEvent<ActionScope>()).Add( action );
	public void AtEndOfThisAction( Action<ActionScope> action ) => (_endOfThisAciton ??= new AsyncEvent<ActionScope>()).Add( action );

	AsyncEvent<ActionScope> _endOfThisAciton;

	#region private
	Dictionary<string, object> dict;
	#endregion
}


public enum ActionCategory {

	Default, // nothing

	// Spirit
	Spirit_Growth,
	Spirit_Power,
	Spirit_SpecialRule, // which specified After X, do Y
	Spirit_PresenceTrackIcon,
	//	GainEnery, // specifiec on preence track
	//	SpiritualRituals,

	Invader,
	//	One Ravage, Build, or Explore in one land

	Blight,
	//	The effects of a Blight Card

	Fear,
	//	Everything one Fear Card does

	Event,
	//	Everything a Main Event Does
	//	Everything a Token Event does
	//	Everything a dahan event does

	Adversary,
	//	An adversary's escalation effects (except englind as it invokes a bild)
	//	Instructions on an adversary to perform some effect.
	//	Actions written on the scenario panel.

	Special
    // Command the Beasts

}