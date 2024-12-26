#nullable enable
namespace SpiritIsland;

/// <summary>
/// A Spirit Island 'Action'
/// </summary>
public sealed class ActionScope : IAsyncDisposable {

	#region static AsyncContainer

	/// <summary> Call this from the root ExecutionContext to initialize. </summary>
	static public void Initialize(ActionScope rootSccope) {
		_scopeContainer.Value = new ActionScopeContainer(rootSccope);
	}

	// This is not really the scope container (AsyncContainer).
	// It is a pointer into the Synchronization Context that holds the container.
	readonly static AsyncLocal<ActionScopeContainer> _scopeContainer = new();

	// this is the actual scope container (AsyncContainer) and it will be null until ActionScope.Initialize is called;
	// this gets shallow-copied into child calls and post-awaited states.
	static ActionScopeContainer Container => _scopeContainer.Value 
		?? throw new InvalidOperationException("Scope container not initialized.  Call ActionScope.Initialize(GameState.RootScope);");

	public class ActionScopeContainer(ActionScope startingValue) {
		public ActionScope Current = startingValue;
	}

	#endregion AsyncContainer

	#region Start of Action Actions 

	static public List<IRunAtStartOfEveryAction> StartOfActionHandlers {
		get {
			var val = _stargOfActionHandlers.Value;
			return val is not null ? val : (_stargOfActionHandlers.Value = []);
		}
	}
	readonly static AsyncLocal<List<IRunAtStartOfEveryAction>> _stargOfActionHandlers = new(); // value gets shallow-copied into child calls and post-awaited states.

	static Task RunStartOfActionHandlers() {
		var handlers = _stargOfActionHandlers.Value;
		return handlers is null ? Task.CompletedTask 
			: Task.WhenAll(handlers.Select(x => x.Start(Current)).ToArray());
	}

	#endregion Start of Action Actions

	#region Static Public Init/Start methods

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
	public static ActionScope Current => Container.Current;

	public static void ThrowIfMissingCurrent(){
		if (Current == null)
			throw new InvalidOperationException("Can't sync tokens when current ActionScope is null.");
	}


	#endregion static properties

	#region public properties

	public GameState GameState { get; }
	public ActionCategory Category { get; }

	public IMoverFactory MoverFactory {
		get => _moverFactory ??= new DefaultMoverFactory();
		set => _moverFactory = value;
	}
	IMoverFactory? _moverFactory;

	/// <summary> Spirit (if any) that owns the action. Null for non-spirit actions. </summary>
	public Spirit? Owner { get => _owner; set => _owner = value; }
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

		ActionScopeContainer container = Container;
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
	public Space AccessTokens(SpaceSpec space) => GameState.Tokens[space];

	#region Non-Spirit Initialized Action Scoped data

	public bool ContainsKey(string key) => _dict is not null && _dict.ContainsKey(key);

	public T SafeGet<T>(string key, Func<T> initializer) {
		if( ContainsKey(key) )
			return (T)_dict![key];
		var initial = initializer()!;
		SafeSet<T>(key, initial);
		return initial;
	}
	public T SafeGet<T>(string key, T initialValue) {
		if( ContainsKey(key) )
			return (T)_dict![key];
		SafeSet<T>(key, initialValue);
		return initialValue;
	}
	public T? SafeGetNullable<T>(string key) where T : class { return ContainsKey(key) ? (T)_dict![key] : null; }

	public void SafeSetNullable<T>(string key, T? value) {
		if( value is null ) {
			if( ContainsKey(key) )
				_dict!.Remove(key);
		} else
			SafeSet<T>(key,value);
	}
	public void SafeSet<T>(string key, T value) { (_dict ??= [])[key] = value!; }

	// NON-NULL!
	public object this[string key]{
		get => _dict is not null && _dict.TryGetValue(key, out object? obj) ? obj : throw new InvalidOperationException($"{key} was not set");
		set => (_dict??=[])[key] = value;
	}
	void Remove(string key) { _dict?.Remove(key);}

	Dictionary<string, object>? _dict;

	#endregion Non-Spirit Initialized Action Scoped data




	public void Log(Log.ILogEntry entry ) => GameState.Log( entry );

	public void LogDebug( string debugMsg ) => GameState.Log( new Log.Debug( debugMsg ) );

	public override string ToString() {
		return $"{Id} : {Category} : "+Owner?.SpiritName??"";
	}

	public async ValueTask DisposeAsync() {
		if(_endOfThisAciton != null)
			await _endOfThisAciton.InvokeAsync(this);

		var current = Container.Current;
		if(current != this) 
			throw new Exception($"Error SI01: Disposing {Category}/{Id} but .Current is {current.Category}/{current.Id}");
		// SI01 - Scenarios that cause this:
		// 1) An ActionScope is missing a using and doesn't dispose of itself.
		//    The parent goes to dispose of itself and finds the child still set as current
		// 2) During testing... Parent ActionScope created on different thread than child ActionScope.
		//    Exception occurs on Parent thread which tries to dispose bethrow bubbling up the exception
		//    However, child ActionScope is still running and hasn't cleaned itself up yet.
		// 3) The Container was initialized in a child execution context, not the root/parent context.

		Container.Current = _old ?? throw new InvalidOperationException("Can't dispose of the root scope or we would have to call Initilize again to get it going.");
	}

	public void AtEndOfThisAction(Func<ActionScope,Task> action ) => (_endOfThisAciton ??= new AsyncEvent<ActionScope>()).Add( action );

	public void AtEndOfThisAction( Action<ActionScope> action ) => (_endOfThisAciton ??= new AsyncEvent<ActionScope>()).Add( action );

	#region private

	readonly ActionScope? _old;

	AsyncEvent<ActionScope>? _endOfThisAciton;
	TerrainMapper? _terrainMapper;
	Spirit? _owner;
	#endregion
}

public interface IMoverFactory {
	TokenMover Gather(Spirit self, Space space);
	TokenMover Pusher(Spirit self, SourceSelector sourceSelector, DestinationSelector? dest = null);
	DestinationSelector PushDestinations { get; }
}

public class DefaultMoverFactory : IMoverFactory {

	public virtual TokenMover Gather(Spirit self, Space space) 
		=> new Builder().FromAdjacents(space).ToSpaces(space).Build(self, "Gather");

	public virtual TokenMover Pusher(Spirit self, SourceSelector sourceSelector, DestinationSelector? dest = null)
		=> new Builder().From(sourceSelector).To(dest ?? DestinationSelector.Adjacent).Build(self,"Push");

	public virtual DestinationSelector PushDestinations => DestinationSelector.Adjacent;



	class Builder {
		public Builder From(SourceSelector ss) { _sourceSelector = ss; return this; }
		public Builder FromSpaces(params IEnumerable<Space> spaces) { _sourceSelector = new SourceSelector(spaces); return this; }
		public Builder FromAdjacents(Space centralSpace) {  _sourceSelector = new SourceSelector(centralSpace.Adjacent); return this;}

		public Builder To(DestinationSelector ds) { _destinationSelector = ds; return this; }
		public Builder ToSpaces( params IEnumerable<Space> destination ) { _destinationSelector = new DestinationSelector(destination); return this; }
		public Builder ToAdjacents(Space centralSpace) {  _destinationSelector = new DestinationSelector(centralSpace.Adjacent); return this; }

		public TokenMover Build(Spirit spirit, string action) => new TokenMover(spirit,action,_sourceSelector!,_destinationSelector!);

		SourceSelector? _sourceSelector;
		DestinationSelector? _destinationSelector;
	}

}

public class BobMoverFactory : DefaultMoverFactory {

	public override TokenMover Gather(Spirit self, Space space) {
		var mover = base.Gather(self,space);

		

		return mover;
	}
		

	public override TokenMover Pusher(Spirit self, SourceSelector sourceSelector, DestinationSelector? dest = null) {
		var mover = base.Pusher(self,sourceSelector,dest);
		return mover;
	}

}