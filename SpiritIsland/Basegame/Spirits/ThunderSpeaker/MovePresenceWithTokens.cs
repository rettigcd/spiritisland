using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {
	public class MovePresenceWithTokens {

		readonly Spirit spirit;
		readonly string prompt;
		public MovePresenceWithTokens( Spirit spirit, string prompt ) { this.prompt = prompt; this.spirit = spirit; }
		public async Task CheckForMove( GameState _, TokenMovedArgs args ) {
			int maxThatCanMove = Math.Min( args.count, spirit.Presence.CountOn( args.from ) );
			// 0 -> no action
			if(maxThatCanMove == 0) return;
			var moveLookup = new Dictionary<string, int>();
			for(int i = maxThatCanMove; 0 < i; --i)
				moveLookup.Add( $"Move {i} presence.", i );
			moveLookup.Add( "stay", 0 );

			string s = await spirit.SelectText( prompt, moveLookup.OrderByDescending( p => p.Value ).Select( p => p.Key ).ToArray() );
			int countToMove = moveLookup[s];

			while(countToMove-- > 0)
				spirit.Presence.Move( args.from, args.to );
		}

	}

}
