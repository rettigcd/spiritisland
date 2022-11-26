namespace SpiritIsland;

// used both for thunderspeaker and sharp fangs
public class MovePresenceWithTokens {

	readonly Spirit spirit;
	readonly TokenClass tokenGroup;

	public MovePresenceWithTokens( Spirit spirit, TokenClass group ) { 
		this.spirit = spirit; 
		this.tokenGroup = group;
	}

	public async Task CheckForMove( ITokenMovedArgs args ) {
		if( args.Class != tokenGroup) return;

		int maxThatCanMove = Math.Min( args.Count, spirit.Presence.CountOn( args.RemovedFrom ) );
		// 0 -> no action
		if(maxThatCanMove == 0)
			return;

		if(maxThatCanMove>1)
			throw new InvalidOperationException("Method is only designed to accept 1 move at a time.");

		// Using 'Gather' here so user can click on existing Presence in Source
		// If we used 'Push', user would click on Destination instead of Source
		var source = await spirit.Gateway.Decision( Select.DeployedPresence.Gather("Move presence with "+ args.Class.Label+"?", args.AddedTo.Space, new SpaceState[]{ args.RemovedFrom } ) );
		if( source != null )
			await spirit.Bind( args.GameState, args.ActionId ).Presence.Move( args.RemovedFrom.Space, args.AddedTo.Space );
	}

}