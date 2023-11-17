namespace SpiritIsland;

public sealed class TokenMover {

	#region Static Factories

	/// <summary> Routes to Pusher on the SpaceState (because that is overridable by Spirit powers) </summary>
	static public TokenMover Push(Spirit self, SpaceState source) => source.Pusher(self);

	/// <summary> Routes to Gatherer on the SpaceState (because that is overridable by Spirit powers) </summary>
	static public TokenMover Gather( Spirit self, SpaceState destination ) => destination.Gather(self);

	static public TokenMover SingleDestination( TargetSpaceCtx ctx, params SpaceState[] sources ) => new TokenMover( ctx.Self, "Move", sources, ctx.Tokens );

	#endregion

	/// <summary>
	/// Convenience constructor for 1 source space moving
	/// </summary>
	public TokenMover(
		Spirit self,
		string actionWord,
		SpaceState sourceSpace,
		params SpaceState[] destinationSpaces
	) : this( self, actionWord, new SourceSelector( sourceSpace ), new DestinationSelector(destinationSpaces )) { }

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

	readonly Spirit _self;
	readonly string _actionWord;
	Present _upToNPresent = Present.Done;

	public Task DoUpToN() => DoN( _upToNPresent );

	async public Task DoN( Present present = Present.Always ) {
		string actionPromptPrefix = present == Present.Always ? _actionWord
			: _actionWord + " up to";
		while(await Do1(actionPromptPrefix,present));
	}

	async Task<bool> Do1(string prompt, Present present) {
		// Select source
		SpaceToken sourceToken = await _sourceSelector.GetSource( _self, prompt, present, _destinationSelector.Single );
		if(sourceToken == null) return false;

		TokenMovedArgs tokenMoved = await MoveSomewhereAsync( sourceToken );
		if(tokenMoved == null) return false;

		await NotifyAsync( tokenMoved );
		return true;
	}

	/// <remarks>
	/// The only time this needs called individually is if the token was already selected 
	/// such as when token is picked during selection of lands also.
	/// </remarks>
	public async Task<TokenMovedArgs> MoveSomewhereAsync( SpaceToken spaceToken ) {

		Space destination = await _destinationSelector.SelectDestination( _self, _actionWord, spaceToken );
		return destination == null ? null
			: await spaceToken.Token.Move( spaceToken.Space.Tokens, destination );
	}

	readonly SourceSelector _sourceSelector;
	readonly DestinationSelector _destinationSelector;
	
	#region Config

	public TokenMover Config( Action<TokenMover> configuration ) { configuration( this); return this;}

	public TokenMover RunAtMax(bool runAtMax) { _upToNPresent = runAtMax ? Present.Always : Present.Done; return this; }

	// Config - Quota
	public TokenMover AddGroup( int count, params ITokenClass[] classes ) { _sourceSelector.AddGroup( count, classes ); return this; }
	public TokenMover AddAll( params ITokenClass[] classes ) { _sourceSelector.AddAll( classes ); return this; }
	public TokenMover UseQuota( Quota quota ) { _sourceSelector.UseQuota( quota ); return this; }

	// Config - Source
	public TokenMover FilterSource( Func<SpaceState, bool> filterSource ) { _sourceSelector.FilterSource(filterSource); return this; }

	/// Config - Destination
	public TokenMover FilterDestination( Func<SpaceState, bool> filterDestination ) { _destinationSelector.FilterDestination(filterDestination); return this; }
	public TokenMover FilterDestinationGroup( Func<IEnumerable<SpaceState>, IEnumerable<SpaceState>> filterGroup ) { _destinationSelector.FilterDestinationGroup( filterGroup ); return this; }

	#endregion

	#region Event / Callback

	public TokenMover Track( Action<TokenMovedArgs> onMoved ) {
		_onMoved.Add( (x)=>{ onMoved(x); return Task.CompletedTask; } );
		return this;
	}

	public TokenMover Track( Func<TokenMovedArgs,Task> onMoved ) {
		_onMoved.Add(onMoved);
		return this;
	}

	async Task NotifyAsync( TokenMovedArgs moveResult ) {
		foreach(var onMoved in _onMoved)
			await onMoved( moveResult );
	}

	List<Func<TokenMovedArgs, Task>> _onMoved = new();

	#endregion

}

static public class MoveFrom {

	static public void ASingleLand( TokenMover mover ) {
		Space source = null;
		mover
			.Track( theMove => source ??= theMove.From.Space )
			.FilterSource( spaceState => source is null || spaceState.Space == source );
	}

}
