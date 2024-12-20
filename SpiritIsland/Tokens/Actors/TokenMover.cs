namespace SpiritIsland;

public sealed class TokenMover(
	Spirit self,
	string actionWord,
	SourceSelector sourceSelector,
	DestinationSelector destinationSelector
) {

	#region Static Factories

	/// <summary> Routes to Gatherer on the Space (because that is overridable by Spirit powers) </summary>
	static public TokenMover Gather( Spirit self, Space destination ) => destination.Gather(self);

	static public TokenMover SingleDestination( TargetSpaceCtx ctx, params Space[] sources ) => new TokenMover( ctx.Self, "Move", sources, ctx.Space );

	#endregion

	#region constructors

	/// <summary>
	/// Convenience constructor for 1 source space moving
	/// </summary>
	public TokenMover(
		Spirit self,
		string actionWord,
		Space sourceSpace,
		params Space[] destinationSpaces
	) : this( self, actionWord, sourceSpace.SourceSelector, new DestinationSelector(destinationSpaces )) { }

	public TokenMover( 
		Spirit self, 
		string actionWord,
		IEnumerable<Space> sourceSpaces,
		params Space[] destinationSpaces
	):this(self,actionWord,new SourceSelector( sourceSpaces ), new DestinationSelector(destinationSpaces)) {}

	#endregion constructors

	public Task DoUpToN() => DoN( _upToNPresent );

	async public Task DoN( Present present = Present.Always ) {

		// await OldWay( present, actionPromptPrefix );
		await NewWay( present );
	}


	async Task NewWay( Present present ) {

		var move = await GetMoveDecision(present);

		while( move is not null) {
			// Notify/Update Source
			await sourceSelector.NotifyAsync(move.Source);
			await destinationSelector.NotifyAsync(move.Destination);

			// Do Move
			TokenMovedArgs? tokenMoved = await move.Apply(1);
			if( tokenMoved != null )
				await NotifyAsync( tokenMoved );

			// next
			move = await GetMoveDecision(present);
		}
	}

	async Task<Move?> GetMoveDecision(Present present) {

		var sourcePromptBuilder = Prompt.RemainingParts( present == Present.Always ? actionWord : actionWord + " up to" );
		A.SpaceTokenDecision srcDecision = sourceSelector.BuildDecision( sourcePromptBuilder, present, destinationSelector.Single, 0, null );

		// Drag and Drop way
		Move[] options = sourceSelector.GetSourceOptions()
			.BuildMoves(destinationSelector.GetDestinationOptions)
			.ToArray();

		return await self.Select(new A.MoveDecision(srcDecision.Prompt,options,present));

	}

	#region Config

	/// <summary> Used for Gathering </summary>
	public TokenMover ConfigSource( Func<SourceSelector,SourceSelector> configuration ) { configuration(sourceSelector); return this;}
	public TokenMover ConfigDestination( Action<DestinationSelector> configure ) { configure(destinationSelector); return this; }

	public TokenMover RunAtMax(bool runAtMax) { _upToNPresent = runAtMax ? Present.Always : Present.Done; return this; }

	// Config - Quota
	public TokenMover AddGroup( int count, params ITokenClass[] classes ) { sourceSelector.AddGroup( count, classes ); return this; }
	public TokenMover AddAll( params ITokenClass[] classes ) { sourceSelector.AddAll( classes ); return this; }
	public TokenMover UseQuota( Quota quota ) { sourceSelector.UseQuota( quota ); return this; }

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

	readonly List<Func<TokenMovedArgs, Task>> _onMoved = [];

	#endregion
	Present _upToNPresent = Present.Done;
}


static public class Move_Extension { 

	static public async Task<TokenMovedArgs?> MoveAsync( this IToken token, ILocation from, ILocation to, int count=1 ) {
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
		ActionScope.Current.Log( tokenMoved );

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

	static public Task<ITokenRemovedArgs> RemoveAsync( this TokenLocation tokenOn, int count=1, RemoveReason reason = RemoveReason.Removed )
		=> tokenOn.Location.RemoveAsync(tokenOn.Token,count,reason);
		
	static public Task<TokenMovedArgs?> MoveToAsync( this TokenLocation tokenOn, ILocation destination, int count=1 )
		=> tokenOn.Token.MoveAsync(tokenOn.Location,destination,count);
}
