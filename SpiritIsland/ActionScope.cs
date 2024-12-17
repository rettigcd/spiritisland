namespace SpiritIsland;

/// <summary>
/// A Spirit Island 'Action'
/// </summary>
public sealed class ActionScope : IAsyncDisposable {

	#region static Container property

	// value gets shallow-copied into child calls and post-awaited states.
	public readonly static AsyncLocal<AsyncContainer> _scopeContainer = new();

	public class AsyncContainer(ActionScope startingValue) {
		public ActionScope Current = startingValue;
	}

	#endregion static Container property

	#region Start of Action Actions 

	//	public static List<IRunAtStartOfAction> StartOfActionHandlers => Container.StartOfActionHandlers;
	static public List<IRunAtStartOfEveryAction> StartOfActionHandlers {
		get {
			var val = _stargOfActionHandlers.Value;
			return val is not null ? val : (_stargOfActionHandlers.Value = []);
		}
	}
	readonly static AsyncLocal<List<IRunAtStartOfEveryAction>> _stargOfActionHandlers = new(); // value gets shallow-copied into child calls and post-awaited states.

	static Task RunStartOfActionHandlers() {
		IRunAtStartOfEveryAction[] snapshop = [.. StartOfActionHandlers]; // so handlers can modify the List
		return Task.WhenAll(snapshop.Select(x => x.Start(Current)));

	}

	#endregion Start of Action Actions

	#region Static Public Init/Start methods

	/// <summary> Call this from the root ExecutionContext to initialize. </summary>
	static public void Initialize( ActionScope rootSccope ) {
		_scopeContainer.Value = new AsyncContainer( rootSccope );
	}

	/// <summary> Starts Spirit Action </summary>
	public static async Task<ActionScope> StartSpiritAction(ActionCategory cat, Spirit spirit) {
		VerifyActionCategory(cat, true, nameof(StartSpiritAction));

		ActionScope scope = new ActionScope(cat,spirit);
		await RunStartOfActionHandlers(); // Outside constructor because it is ASYNC
		return scope;
	}

	/// <summary> Starts a Non-Spirit action </summary>
	public static async Task<ActionScope> Start(ActionCategory cat) {
		VerifyActionCategory(cat, false, nameof(Start));

		ActionScope scope = new ActionScope(cat);
		await RunStartOfActionHandlers(); // Outside constructor because it is ASYNC
		return scope;
	}

	/// <summary> For Testing only </summary>
	public static ActionScope Start_NoStartActions(ActionCategory cat) => new ActionScope(cat);

	#endregion Static Public Init/Start methods

	#region static properties

	// Good Reading on Execution Context and async/await
	// https://devblogs.microsoft.com/pfxteam/executioncontext-vs-synchronizationcontext/
	// https://stackoverflow.com/questions/39795286/does-async-await-increases-context-switching
	// https://learn.microsoft.com/en-us/dotnet/api/system.threading.asynclocal-1?view=net-7.0
	// https://nelsonparente.medium.com/a-little-riddle-with-asynclocal-1fd11322067f
	public static ActionScope Current => _scopeContainer?.Value?.Current; // returns null if no ActionScope

	public static void ThrowIfMissingCurrent(){
		if (Current == null)
			throw new InvalidOperationException("Can't sync tokens when current ActionScope is null.");
	}


	#endregion static properties

	#region public properties

	public GameState GameState { get; }
	public ActionCategory Category { get; }
	public Func<Space, Space> Upgrader { get => _upgrader; set => _upgrader = value; }
	/// <summary> Spirit (if any) that owns the action. Null for non-spirit actions. </summary>
	public Spirit Owner { get => _owner; set => _owner = value; }
	public Guid Id { get; }
	/// <summary> Non-stasis + InPlay </summary>
	public IEnumerable<Space> Spaces => GameState.SpaceSpecs.Select(AccessTokens);
	/// <summary> All Non-stasis (even not-in-play) </summary>
	public IEnumerable<Space> Spaces_Existing => GameState.SpaceSpecs_Existing.Select(AccessTokens);
	public IEnumerable<Space> Spaces_Unfiltered => GameState.SpaceSpecs_Unfiltered.Select(AccessTokens);
	public TerrainMapper TerrainMapper => _terrainMapper ??= (GameState?.GetTerrain(Category) ?? new TerrainMapper()); // If not GameState / configuration, use default

	#endregion public properties

	#region constructor

	/// <summary> Root Scope </summary>
	public ActionScope( GameState gameState ) {
		Id = Guid.NewGuid();
		Category = ActionCategory.Default;
		GameState = gameState;
	}

	ActionScope(ActionCategory actionCategory, Spirit spirit):this(actionCategory) {
		Owner = spirit ?? throw new ArgumentNullException(nameof(spirit));
		spirit.InitSpiritAction(this);
	}

	/// <summary> Spirit action </summary>
	ActionScope(ActionCategory actionCategory) {
		Id = Guid.NewGuid();
		Category = actionCategory;

		AsyncContainer container = _scopeContainer.Value; // grab containers
		_old = container.Current;
		container.Current = this;

		GameState = _old.GameState;
	}

	static void VerifyActionCategory( ActionCategory cat, bool expectedIsSpiritActin, string constructorName ) {
		bool isSpiritAction 
			 = cat == ActionCategory.Spirit_Growth 
			|| cat == ActionCategory.Spirit_PresenceTrackIcon
			|| cat == ActionCategory.Spirit_Power;
			
		if( isSpiritAction != expectedIsSpiritActin )
			throw new InvalidOperationException($"{cat} category should not be used with {constructorName}.");
	}

	#endregion

	/// <summary> Called from Space.Tokens to get Tokens. </summary>
	/// <remarks> Provides hook for spirits to modify the Space object used for their actions.</remarks>
	public Space AccessTokens(SpaceSpec space) => Upgrader( GameState.Tokens[space] );

	#region Non-Spirit Initialized Action Scoped data

	// Generic String / object dictionary to track action things

	public bool ContainsKey(string key) => _dict != null && _dict.ContainsKey( key );

	public T SafeGet<T>(string key, T defaultValue=default) => ContainsKey(key) ? (T) this[key] : defaultValue;

	public T SafeGet<T>( string key, Func<T> initFunc ) {  
		if(ContainsKey( key )) return (T)this[key];
		T newValue = initFunc();
		this[key] = newValue;
		return newValue;
	}

	public object this[string key]{
		get => ContainsKey(key) ? _dict[key] : throw new InvalidOperationException($"{key} was not set");
		set => (_dict??= [])[key] = value;
	}

	Dictionary<string, object> _dict;

	#endregion Non-Spirit Initialized Action Scoped data

	public void Log(Log.ILogEntry entry ) => GameState.Log( entry );

	public void LogDebug( string debugMsg ) => GameState.Log( new Log.Debug( debugMsg ) );

	public override string ToString() {
		return $"{Id} : {Category} : "+Owner?.SpiritName??"";
	}

	public async ValueTask DisposeAsync() {
		if(_endOfThisAciton != null)
			await _endOfThisAciton.InvokeAsync(this);

		var current = _scopeContainer.Value.Current;
		if(current != this) 
			throw new Exception($"Error SI01: Disposing {Category}/{Id} but .Current is {current.Category}/{current.Id}");
		// SI01 - Scenarios that cause this:
		// 1) An ActionScope is missing a using and doesn't dispose of itself.
		//    The parent goes to dispose of itself and finds the child still set as current
		// 2) During testing... Parent ActionScope created on different thread than child ActionScope.
		//    Exception occurs on Parent thread which tries to dispose bethrow bubbling up the exception
		//    However, child ActionScope is still running and hasn't cleaned itself up yet.
		// 3) The Container was initialized in a child execution context, not the root/parent context.

		_scopeContainer.Value.Current = _old;
	}

	public void AtEndOfThisAction(Func<ActionScope,Task> action ) => (_endOfThisAciton ??= new AsyncEvent<ActionScope>()).Add( action );

	public void AtEndOfThisAction( Action<ActionScope> action ) => (_endOfThisAciton ??= new AsyncEvent<ActionScope>()).Add( action );

	#region private

	readonly ActionScope _old;

	AsyncEvent<ActionScope> _endOfThisAciton;
	TerrainMapper _terrainMapper;
	Func<Space, Space> _upgrader = ss=>ss;
	Spirit _owner;
	#endregion
}
