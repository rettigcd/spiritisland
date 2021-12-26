using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class AllThingsWeaken : BlightCardBase {

		public AllThingsWeaken():base("All Things Weaken",3) {}

		protected override Task BlightAction( GameState gs ) {
			// Ongoing, starting next turn:
			gs.TimePasses_ThisRound.Push( gs => {
				ReduceTokenHealthBy1( gs );

				// The land takes blight on 1 less Damage (normally 1),.
				gs.DamageToBlightLand = 1;

				// When you add blight, it Destroys all presence/beast in that land and 1 presence (total) in an adjacent land.
				// !!! implement this
				return Task.CompletedTask;
			} );

			return Task.CompletedTask;
		}

		private static void ReduceTokenHealthBy1( GameState gs ) {
			// Invaders and Dahan have -1 Health (min, 1).
			var replacements = new List<(Token, Token)>();
			foreach(var invaderClass in new[] { Invader.Town, Invader.City, TokenType.Dahan }) {
				var oldDefault = invaderClass.Default;
				if(oldDefault.Health == 1) continue;
				invaderClass.RemoveTopDefault();
				var newDefault = invaderClass.Default;
				replacements.Add( (oldDefault, newDefault) );
			}
			foreach(var space in gs.Island.AllSpaces) {
				var tokens = gs.Tokens[space];
				foreach(var (from, to) in replacements) {
					tokens.Adjust( to, tokens[from] );
					tokens.Init( from, 0 );
				}
			}
		}
	}

}
