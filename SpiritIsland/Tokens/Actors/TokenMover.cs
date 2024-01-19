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

		// await OldWay( present, actionPromptPrefix );
		await NewWay( present );
	}


	async Task NewWay( Present present ) {

		while(true) {

			var move = await GetMoveDecision(present);
			if(move == null) break;

			// Do Move
			TokenMovedArgs tokenMoved = await move.Source.MoveTo( move.Destination );

			// Notify/Update Source
			await _sourceSelector.NotifyAsync( move.Source );
			await _destinationSelector.NotifyAsync( move.Destination );
			await NotifyAsync( tokenMoved );
		}
	}

	async Task<Move> GetMoveDecision(Present present) {

		var sourcePromptBuilder = Prompt.RemainingParts( present == Present.Always ? _actionWord : _actionWord + " up to" );
		A.SpaceToken srcDecision = _sourceSelector.BuildDecision( sourcePromptBuilder, present, _destinationSelector.Single, 0, null );

		// Drag and Drop way
		Move[] options = _sourceSelector.GetSourceOptions()
			.SelectMany( s => _destinationSelector.GetDestinationOptions(s).Select(d=>new Move {Source=s,Destination=d } ))
			.ToArray();
		return await _self.SelectAsync(new A.Move(srcDecision.Prompt,options,present));

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


static public class Move_Extension { 

	static public async Task<TokenMovedArgs> MoveAsync( this IToken token, ILocation from, ILocation to, int count=1 ) {
		// Current implementation favors:
		//		switching token types prior to Add/Remove so events handlers don't switch token type
		//		perfoming the add/remove action After the Adding/Removing modifications

		// Possible problems to keep in mind:
		//		The token in the Added event, may be different than token that was attempted to be added.
		//		The Token in the Removed event, may be a different token than was requested to be removed.
		//		The token Added may be Different than the token Removed
		//		Move requires a special Publish because it pertains to 2 spaces - we don't want to publish it twice (once for each space)

		// Mitigating Factors
		//		The AddingToken args prevents changing the Count if it is a MoveTo

		// Remove from source
		var (removeResult,removedNotifier) = await from.SourceAsync( token, count, RemoveReason.MovedFrom );
		if( removeResult.Count == 0 ) return null;

		// Add to destination
		var (addResult,addedNotifier) = await to.SinkAsync( 
			removeResult.Removed, // possibly modified, NOT original
			removeResult.Count, // possibly modified
			AddReason.MovedTo
		);

		// Publish
		var tokenMoved = new TokenMovedArgs( removeResult, addResult );
		await removedNotifier( tokenMoved );
		await addedNotifier( tokenMoved );

		return tokenMoved;
	}

	/// <summary>Removes a token from a Location</summary>
	/// <returns>null if no token removed</returns>
	static public async Task<ITokenRemovedArgs> RemoveAsync( this ILocation source, IToken token, int count=1, RemoveReason reason = RemoveReason.Removed ) {
		if( reason == RemoveReason.MovedFrom )
			throw new ArgumentException("Moving Tokens must be done from the .Move method for events to work properly",nameof(reason));

		var (removed,removedHandler) = await source.SourceAsync( token, count, reason );
		if( 0<removed.Count )
			await removedHandler(removed);

		return removed;
	}

	static public Task<ITokenRemovedArgs> RemoveAsync( this TokenOn tokenOn, int count=1, RemoveReason reason = RemoveReason.Removed )
		=> tokenOn.Source.RemoveAsync(tokenOn.Token,count,reason);
	static public Task<TokenMovedArgs> MoveToAsync( this TokenOn tokenOn, ILocation destination, int count=1 )
		=> tokenOn.Token.MoveAsync(tokenOn.Source,destination,count);
}
