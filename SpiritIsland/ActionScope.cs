using System.ComponentModel;

namespace SpiritIsland;

/// <summary>
/// A Spirit Island 'Action'
/// </summary>
public sealed class ActionScope : IAsyncDisposable {

	#region scope container

	/// <summary> Call this from the root ExecutionContext to initialize. </summary>
	static public void Initialize() {
		_scopeContainer.Value = new ActionScopeContainer();
	}

	class ActionScopeContainer {
		public ActionScopeContainer() {
			Id = Guid.NewGuid();
			Current = new ActionScope(this); // default
			StartOfActionHandlers = new List<IRunAtStartOfAction>();
		}
		readonly Guid Id;
		public ActionScope Current; // default
		public readonly List<IRunAtStartOfAction> StartOfActionHandlers;
		public override string ToString() => Id.ToString();
		public Task RunStartOfActionHandlers() {
			IRunAtStartOfAction[] snapshop = StartOfActionHandlers.ToArray(); // so handlers can modify the List
			return Task.WhenAll( snapshop.Select( x => x.Start( Current ) ) );

		}
	}

	// Good Reading on Execution Context and async/await
	// https://devblogs.microsoft.com/pfxteam/executioncontext-vs-synchronizationcontext/
	// https://stackoverflow.com/questions/39795286/does-async-await-increases-context-switching
	// https://learn.microsoft.com/en-us/dotnet/api/system.threading.asynclocal-1?view=net-7.0
	// https://nelsonparente.medium.com/a-little-riddle-with-asynclocal-1fd11322067f
	public static ActionScope Current => Container.Current;
	public static List<IRunAtStartOfAction> StartOfActionHandlers => Container.StartOfActionHandlers;
	static ActionScopeContainer Container => _scopeContainer.Value ?? throw new InvalidOperationException( NotInitializedMessage );
	readonly static AsyncLocal<ActionScopeContainer> _scopeContainer = new AsyncLocal<ActionScopeContainer>(); // value gets shallow-copied into child calls and post-awaited states.

	const string NotInitializedMessage = "ActionScope not initialized. Call ActionScope.Initialize() from root ExecutionContext.";

	#endregion

	#region constructor

	// The default / game-starting scope

	ActionScope( ActionScopeContainer container ) {
		Id = Guid.NewGuid();
		_container = container;
		_neverCache = true; // !! Since it is a singleton, make usable across multiple execution contexts
	}

	/// <summary> For Testing only </summary>
	public static ActionScope Start_NoStartActions( ActionCategory cat ) => new ActionScope( cat, Container );

	public static async Task<ActionScope> Start( ActionCategory cat ) {
		ActionScopeContainer container = Container;				// grab from ExecutionContext
		ActionScope scope = new ActionScope( cat, container );  // start new (and assign current)
		await container.RunStartOfActionHandlers();
		return scope;
	}

	ActionScope( ActionCategory actionCategory, ActionScopeContainer container ) {
		Id = Guid.NewGuid();
		Category = actionCategory;
		_container = container;

		_old = _container.Current;
		_container.Current = this;
	}
	#endregion

	public ActionCategory Category { get; }

	public GameState GameState => _neverCache ? GameState.Current : _gameState ??= GameState.Current;
	
	/// <summary> Called from Space.Tokens to get Tokens. </summary>
	/// <remarks> Provides hook for spirits to modify the SpaceState object used for their actions.</remarks>
	public SpaceState AccessTokens(Space space) => _upgrader( GameState.Tokens[space] );
	public Func<SpaceState, SpaceState> Upgrader {
		set {
			if(_neverCache) throw new InvalidOperationException( "Can't set owner on default scope" );
			_upgrader = value;
		}
	}

	public TerrainMapper TerrainMapper => _terrainMapper 
		??= (GameState?.GetTerrain( Category ) ?? new TerrainMapper()); // If not GameState / configuration, use default

	/// <summary>
	/// Spirit (if any) that owns the action. Null for non-spirit actions.
	/// </summary>
	public Spirit Owner { 
		get => _owner;
		set { if(_neverCache) throw new InvalidOperationException("Can't set owner on default scope"); _owner = value; }
	}

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

		var current = _container.Current;
		if(current != this) 
			throw new Exception($"Error SI01: Disposing {Category}/{Id} but .Current is {current.Category}/{current.Id}");
			// SI01 - Scenarios that cause this:
			// 1) An ActionScope is missing a using and doesn't dispose of itself.
			//    The parent goes to dispose of itself and finds the child still set as current
			// 2) During testing... Parent ActionScope created on different thread than child ActionScope.
			//    Exception occurs on Parent thread which tries to dispose bethrow bubbling up the exception
			//    However, child ActionScope is still running and hasn't cleaned itself up yet.
			// 3) The Container was initialized in a child execution context, not the root/parent context.

		_container.Current = _old;
	}

	public void AtEndOfThisAction(Func<ActionScope,Task> action ) => (_endOfThisAciton ??= new AsyncEvent<ActionScope>()).Add( action );
	public void AtEndOfThisAction( Action<ActionScope> action ) => (_endOfThisAciton ??= new AsyncEvent<ActionScope>()).Add( action );

	#region private
	readonly ActionScopeContainer _container;
	readonly bool _neverCache = false; // for the root Action...
	readonly ActionScope _old;
	Dictionary<string, object> dict;
	AsyncEvent<ActionScope> _endOfThisAciton;
	TerrainMapper _terrainMapper;
	GameState _gameState;
	Func<SpaceState, SpaceState> _upgrader = ss=>ss;
	Spirit _owner;
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
