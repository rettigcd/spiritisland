using System;
using System.Threading.Tasks;

namespace SpiritIsland {

	// used both for thunderspeaker and sharp fangs
	public class MovePresenceWithTokens {

		readonly Spirit spirit;
		readonly string prompt;
		readonly TokenGroup tokenGroup;
		public MovePresenceWithTokens( Spirit spirit, string prompt, TokenGroup group ) { 
			this.prompt = prompt; 
			this.spirit = spirit; 
			this.tokenGroup = group;
		}
		public async Task CheckForMove( GameState _, TokenMovedArgs args ) {
			if( args.Token.Generic != tokenGroup) return;

			int maxThatCanMove = Math.Min( args.count, spirit.Presence.CountOn( args.from ) );
			// 0 -> no action
			if(maxThatCanMove == 0)
				return;

			if(maxThatCanMove>1)
				throw new InvalidOperationException("Method is only designed to accept 1 move at a time.");

			var source = await spirit.Action.Decision( new Decision.Presence.Deployed("Move presence with "+args.Token.Generic.Label+"?", new Space[]{ args.from }, Present.Done ) );
			if( source != null )
				spirit.Presence.Move( args.from, args.to );
		}

	}

}
