namespace SpiritIsland;

public sealed class TokenMover(
	Spirit self,
	string actionWord,
	SourceSelector sourceSelector,
	DestinationSelector destinationSelector
) {

	#region Static Factories

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

		Move? move = await GetMoveDecision(present);

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

		// Intensify Hook
		if(DoEndStuff is not null)
			await DoEndStuff(_ranOutOfOptions,_firstMove!,self);
	}

	public event Func<bool, Move[], Spirit, Task>? DoEndStuff;

	async Task<Move?> GetMoveDecision(Present present) {

		var sourcePromptBuilder = Prompt.RemainingParts( present == Present.Always ? actionWord : actionWord + " up to" );

		string prompt = sourcePromptBuilder( sourceSelector.PromptData( 0, null ) );

		// Drag and Drop way
		Move[] options = sourceSelector.GetSourceOptions()
			.BuildMoves(destinationSelector.GetDestinationOptions)
			.ToArray();

		if(_firstMove == null)
			_firstMove = options;

		if(options.Length == 0 ) {
			_ranOutOfOptions = true;
			return null;
		}

		return await self.Select(new A.MoveDecision(prompt,options,present));

	}

	Move[]? _firstMove = null; // Hook/Helper for Intensify
	bool _ranOutOfOptions = false; // Hook/Helper for Intensify

	#region Config

	/// <summary> Used for Gathering </summary>
	public TokenMover ConfigSource( Func<SourceSelector,SourceSelector> configuration ) { configuration(sourceSelector); return this;}
	public TokenMover ConfigDestination( Action<DestinationSelector> configure ) { configure(destinationSelector); return this; }

	public TokenMover RunAtMax(bool runAtMax) { _upToNPresent = runAtMax ? Present.Always : Present.Done; return this; }

	// Config - Quota
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
