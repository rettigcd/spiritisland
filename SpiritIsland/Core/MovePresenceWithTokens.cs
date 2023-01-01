namespace SpiritIsland;

// used both for thunderspeaker and sharp fangs
public class MovePresenceWithTokens {

	public MovePresenceWithTokens( Spirit spirit, TokenClass tokenClass ) { 
		_spirit = spirit; 
		_tokenClass = tokenClass;
	}

	public async Task CheckForMove( ITokenMovedArgs args ) {
		if( args.TokenRemoved.Class != _tokenClass ) return;
		if( !_spirit.Presence.HasMovableTokens(args.RemovedFrom) ) return;

		int maxThatCanMove = Math.Min( args.Count, _spirit.Presence.CountOn( args.RemovedFrom ) );
		// 0 -> no action
		if(maxThatCanMove == 0)
			return;

		if(maxThatCanMove>1)
			throw new InvalidOperationException("Method is only designed to accept 1 move at a time.");

		// Using 'Gather' here so user can click on existing Presence in Source
		// If we used 'Push', user would click on Destination instead of Source
		var source = await _spirit.Gateway.Decision( Select.DeployedPresence.Gather("Move presence with "+ args.TokenRemoved.Class.Label+"?", args.AddedTo.Space, new SpaceState[]{ args.RemovedFrom } ) );
		if( source != null )
			await _spirit.BindSelf( args.GameState, args.UnitOfWork ).Presence.Move( args.RemovedFrom.Space, args.AddedTo.Space );
	}

	#region private field
	readonly Spirit _spirit;
	readonly TokenClass _tokenClass;
	#endregion

}