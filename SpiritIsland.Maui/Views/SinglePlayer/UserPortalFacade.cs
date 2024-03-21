namespace SpiritIsland.Maui;

/// <summary>
/// Wraps the User Portal.
/// Breaks down Moves into 2-step processes that are easier for UI to handle.
/// </summary>
internal class UserPortalFacade : IDecisionPortal {

	public event Action<IDecision>? NewWaitingDecision;

	#region constructor

	public UserPortalFacade(IDecisionPortal inner) {
		_inner = inner;
		_inner.NewWaitingDecision += Inner_NewWaitingDecision;
	}

	#endregion constructor

	public void Release() {
		_inner.NewWaitingDecision -= Inner_NewWaitingDecision;
	}

	public void Choose( IDecision decision, IOption option, bool block = true ) {
		// If nothing special, just pass to inner
		if(_moveBehavior is null) {
			_inner.Choose( decision, option, block );
			return; // don't return result.
		}

		var nextDecision = _moveBehavior.Handle(option,block);
		if(nextDecision is not null)
			OnNewWaitingDecision( nextDecision );
	}

	#region private Move helpers

	void Inner_NewWaitingDecision( IDecision decision ) {
		// $$$ Call Task.Run because it has lots of stuff to do (UI updates) and 
		// we don't want to hold up the Engine thread. (because that caused it to lock-up somehow)

		Task.Run( () => { 

			// !!! 
			if(decision is A.Move move) {
				// Move
				_moveBehavior = new MoveBehavior( _inner, move );
				decision = _moveBehavior.GetSourceDecision();
			} else
				// Non-Move
				_moveBehavior = null;

			OnNewWaitingDecision( decision );

		} );

	}

	void OnNewWaitingDecision( IDecision decision ) {
		DateTime start = DateTime.Now;

		// non blocking
		_ = MainThread.InvokeOnMainThreadAsync( () => NewWaitingDecision?.Invoke( decision ) );
		// Look into !!! Dispatcher.Dispatch( async () => NewWaitingDecision?.Invoke( decision ) );

		var delta = DateTime.Now - start;
	}

	#endregion private Move helpers

	#region pass-thru

	// Not implementing this because:
	//	* it is troublesome for the 2-part move bit
	//	* it is blocking.
	public IDecision Next => _inner.Next; // !!! DO NOT USE this accept at very beginning.

	// Commands
//	public void IssueException( Exception ex )	=> _inner.IssueException( ex );
//	public void Quit(GameOverException ex) => _inner.Quit(ex);

	// !!! What is the difference in these.
	public bool WaitForNext( int ms ) => _inner.WaitForNext( ms );
	public bool WaitForNextDecision( int milliseconds ) => _inner.WaitForNextDecision( milliseconds );

	#endregion pass-thru

	#region private methods

	MoveBehavior? _moveBehavior;
	readonly IDecisionPortal _inner;

	#endregion

}

/// <summary>
/// Splits up a Move Decision into Source/Destination.
/// </summary>
class MoveBehavior( IDecisionPortal inner, A.Move move ) {
	public IDecision GetSourceDecision() {
		var st = new A.SpaceTokenDecision(
			move.Prompt,
			_moveOptions.Select( s => s.Source ).Distinct(),
			_moveIsOptional ? Present.Done : Present.Always
		);
		var destinations = _moveOptions.Select( s => s.Destination ).Distinct().ToArray();
		if(destinations.Length == 1)
			st.PointArrowTo( destinations[0].SpaceSpec );
		return st;
	}

	public IDecision? Handle(IOption option, bool block) {
		if(option is SpaceToken source)
			return HandleMoveSource( source, block );

		if(option is SpaceSpec destination) {
			HandleMoveDestination( destination, block );
			return null;
		}
		
		if(TextOption.Done == option) {
			inner.Choose( move, option, block );
			return null;
		}

		throw new ArgumentException( "Part 2 of move should be a space" );
	}

	A.SpaceDecision? HandleMoveSource( SpaceToken source, bool block ) {
		// if only 1 destination - Auto-select it now (can't use Present.Auto in the ui)
		Space[] destinationOptions = _moveOptions!
			.Where( s => s.Source == source )
			.Select( s => s.Destination )
			.Distinct()
			.ToArray();
		if(destinationOptions.Length != 1) {
			// Setup TO choice
			_moveSource = source;
			return new A.SpaceDecision(
					"Move to",
					destinationOptions,
					Present.AutoSelectSingle // if they selected a source, don't let them cancel.
				)
				.ComingFrom( source.Space.SpaceSpec )
				.ShowTokenLocation( source.Token );
		} else {
			// Auto-Select-Single
			Move singleMove = _moveOptions.Single( s => s.Source == source && s.Destination == destinationOptions[0] );
			// false - trigger inner
			inner.Choose( move, singleMove, block );
			return null;
		}
	}

	void HandleMoveDestination( SpaceSpec destination, bool block ) {
		Move realOption = _moveOptions
			.Single( s => s.Source == _moveSource && s.Destination.SpaceSpec == destination );
		inner.Choose( move, realOption, block );
	}

	SpaceToken? _moveSource = null;
	readonly Move[] _moveOptions = move.Options.OfType<Move>().ToArray();
	readonly bool _moveIsOptional = move.Options.Any( x => x == TextOption.Done );
}
