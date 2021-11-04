using System;
using System.Threading.Tasks;

namespace SpiritIsland {

	// used both for thunderspeaker and sharp fangs
	public class MovePresenceWithTokens {

		readonly Spirit spirit;
		readonly TokenGroup tokenGroup;

		public MovePresenceWithTokens( Spirit spirit, TokenGroup group ) { 
			this.spirit = spirit; 
			this.tokenGroup = group;
		}

		public async Task CheckForMove( GameState _, TokenMovedArgs args ) {
			if( args.Token.Generic != tokenGroup) return;

			int maxThatCanMove = Math.Min( args.count, spirit.Presence.CountOn( args.From ) );
			// 0 -> no action
			if(maxThatCanMove == 0)
				return;

			if(maxThatCanMove>1)
				throw new InvalidOperationException("Method is only designed to accept 1 move at a time.");

			// Using 'Gather' here so user can click on existing Presence in Source
			// If we used 'Push', user would click on Destination instead of Source
			var source = await spirit.Action.Decision( new Decision.Presence.Gather("Move presence with "+args.Token.Generic.Label+"?", args.To, args.From ) );
			if( source != null )
				spirit.Presence.Move( args.From, args.To );
		}

	}

}
