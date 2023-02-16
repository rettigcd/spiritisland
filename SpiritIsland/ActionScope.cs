using System;
using System.Reflection.Metadata.Ecma335;

namespace SpiritIsland;

/// <summary>
/// A Spirit Island 'Action'
/// </summary>
public sealed class ActionScope : IAsyncDisposable {

	#region scope container
	class ActionScopeContainer {
		public ActionScope Current = new ActionScope(); // default
	}
	static ActionScopeContainer Container => _container.Value ??= new ActionScopeContainer();
	readonly static AsyncLocal<ActionScopeContainer> _container = new AsyncLocal<ActionScopeContainer>(); // value gets shallow-copied into child calls and post-awaited states.
	#endregion

	// https://learn.microsoft.com/en-us/dotnet/api/system.threading.asynclocal-1?view=net-7.0
	// https://nelsonparente.medium.com/a-little-riddle-with-asynclocal-1fd11322067f
	public static ActionScope Current => Container.Current;

	#region constructor

	// The default / game-starting scope

	ActionScope() {
		Id = Guid.NewGuid();
		_neverCache = true; // !! Since it is a singleton, make usable across multiple execution contexts
	}

	public ActionScope( ActionCategory actionCategory ) {
		Id = Guid.NewGuid();
		Category = actionCategory;

		_old = Container.Current;
		Container.Current = this;
	}
	#endregion

	public ActionCategory Category { get; }

	public GameState GameState => _neverCache ? GameState.Current : _gameState ??= GameState.Current;
	GameState _gameState;
	readonly bool _neverCache = false; // for the root Action...

	public SpaceState AccessTokens(Space space) => _upgrader( GameState.Tokens[space] );
	public Func<SpaceState, SpaceState> Upgrader {
		set {
			if(_neverCache) throw new InvalidOperationException( "Can't set owner on default scope" );
			_upgrader = value;
		}
	}
	Func<SpaceState, SpaceState> _upgrader = DefaultUpgrader;
	static SpaceState DefaultUpgrader(SpaceState ss) => ss;

	public TerrainMapper TerrainMapper => _terrainMapper 
		??= (GameState?.GetTerrain( Category ) ?? new TerrainMapper()); // If not GameState / configuration, use default
	TerrainMapper _terrainMapper;

	readonly ActionScope _old;

	// spirit (if any) that owns the action. Null for non-spirit actions
	public Spirit Owner { 
		get => _owner;
		set { if(_neverCache) throw new InvalidOperationException("Can't set owner on default scope"); _owner = value; }
	}
	Spirit _owner;

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

		var container = Container;
		var current = container.Current;
		if(current != this) 
			throw new Exception($"Disposing {Category}/{Id} but .Current is {current.Category}/{current.Id}");

		container.Current = _old;
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