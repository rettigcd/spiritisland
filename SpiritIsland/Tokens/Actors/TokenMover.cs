namespace SpiritIsland;

public sealed class TokenMover {

	#region Static Factories

	/// <summary> Routes to Gatherer on the SpaceState (because that is overridable by Spirit powers) </summary>
	static public TokenMover Gather( Spirit self, SpaceState destination ) => destination.Gather(self);

	static public TokenMover SingleDestination( TargetSpaceCtx ctx, params SpaceState[] sources ) => new TokenMover( ctx.Self, "Move", sources, ctx.Tokens );

	#endregion

	#region constructors

	/// <summary>
	/// Convenience constructor for 1 source space moving
	/// </summary>
	public TokenMover(
		Spirit self,
		string actionWord,
		SpaceState sourceSpace,
		params SpaceState[] destinationSpaces
	) : this( self, actionWord, sourceSpace.SourceSelector, new DestinationSelector(destinationSpaces )) { }

	public TokenMover( 
		Spirit self, 
		string actionWord,
		IEnumerable<SpaceState> sourceSpaces,
		params SpaceState[] destinationSpaces
	):this(self,actionWord,new SourceSelector( sourceSpaces ), new DestinationSelector(destinationSpaces)) {}

	public TokenMover(
		Spirit self,
		string actionWord,
		SourceSelector sourceSelector,
		DestinationSelector destinationSelector
	) {
		_self = self;
		_actionWord = actionWord;
		_sourceSelector = sourceSelector;
		_destinationSelector = destinationSelector;
	}

	#endregion constructors

	public Task DoUpToN() => DoN( _upToNPresent );

	async public Task DoN( Present present = Present.Always ) {
		string actionPromptPrefix = present == Present.Always ? _actionWord
			: _actionWord + " up to";

		IAsyncEnumerable<SpaceToken> sourceTokens = _sourceSelector.GetEnumerator(
			_self,
			Prompt.RemainingParts(actionPromptPrefix),
			present,
			_destinationSelector.Single
		);
		await foreach(SpaceToken sourceToken in sourceTokens)
			await DoSomethingWithSource( sourceToken );
	}

	async Task DoSomethingWithSource( SpaceToken sourceToken ) {
		TokenMovedArgs tokenMoved = await sourceToken.MoveToAsync(_actionWord,_destinationSelector,_self);
		await NotifyAsync( tokenMoved );
	}

	#region Config

	/// <summary> Used for Gathering </summary>
	public TokenMover ConfigSource( Func<SourceSelector,SourceSelector> configuration ) { configuration(_sourceSelector); return this;}
	public TokenMover ConfigDestination( Action<DestinationSelector> configure ) { configure(_destinationSelector); return this; }

	public TokenMover RunAtMax(bool runAtMax) { _upToNPresent = runAtMax ? Present.Always : Present.Done; return this; }

	// Config - Quota
	public TokenMover AddGroup( int count, params ITokenClass[] classes ) { _sourceSelector.AddGroup( count, classes ); return this; }
	public TokenMover AddAll( params ITokenClass[] classes ) { _sourceSelector.AddAll( classes ); return this; }
	public TokenMover UseQuota( Quota quota ) { _sourceSelector.UseQuota( quota ); return this; }

	#endregion

	#region Event / Callback

	public TokenMover Bring( Func<TokenMovedArgs,Task> onMoved ) {
		_onMoved.Add(onMoved);
		return this;
	}

	async Task NotifyAsync( TokenMovedArgs moveResult ) {
		foreach(var onMoved in _onMoved)
			await onMoved( moveResult );
	}

	readonly List<Func<TokenMovedArgs, Task>> _onMoved = new();

	#endregion

	readonly Spirit _self;
	readonly string _actionWord;
	Present _upToNPresent = Present.Done;
	readonly SourceSelector _sourceSelector;
	readonly DestinationSelector _destinationSelector;

}
