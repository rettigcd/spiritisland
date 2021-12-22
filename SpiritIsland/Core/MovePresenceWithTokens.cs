using System;
using System.Threading.Tasks;

namespace SpiritIsland {

	// used both for thunderspeaker and sharp fangs
	public class MovePresenceWithTokens {

		readonly Spirit spirit;
		readonly TokenClass tokenGroup;

		public MovePresenceWithTokens( Spirit spirit, TokenClass group ) { 
			this.spirit = spirit; 
			this.tokenGroup = group;
		}

		public async Task CheckForMove( GameState gs, TokenMovedArgs args ) {
			if( args.Token.Class != tokenGroup) return;

			int maxThatCanMove = Math.Min( args.Count, spirit.Presence.CountOn( args.RemovedFrom ) );
			// 0 -> no action
			if(maxThatCanMove == 0)
				return;

			if(maxThatCanMove>1)
				throw new InvalidOperationException("Method is only designed to accept 1 move at a time.");

			// Using 'Gather' here so user can click on existing Presence in Source
			// If we used 'Push', user would click on Destination instead of Source
			var source = await spirit.Action.Decision( Select.DeployedPresence.Gather("Move presence with "+ args.Token.Class.Label+"?", args.AddedTo, new Space[]{ args.RemovedFrom } ) );
			if( source != null )
				spirit.Presence.Move( args.RemovedFrom, args.AddedTo, gs );
		}

	}

}
