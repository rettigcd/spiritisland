using System.Collections.Generic;

namespace SpiritIsland.JaggedEarth {

	public class AllThingsWeaken : BlightCardBase {

		public AllThingsWeaken():base("All Things Weaken",3) {}

		public override ActionOption<GameState> Immediately => 
			// Ongoing, starting next turn:
			Cmd.AtTheStartOfNextRound( Cmd.Multiple<GameState>(
				// Invaders and Dahan have -1 Health (min, 1).
				ReduceTokenHealthBy1,
				// The land takes blight on 1 less Damage (normally 1).
				TakeBlightOn1LessDamage,
				// When you add blight, it Destroys all presence/beast in that land and 1 presence (total) in an adjacent land.
				AddBlightDestroyesPresence
			));

		static ActionOption<GameState> ReduceTokenHealthBy1 => 

			// Invaders and Dahan have -1 Health (min, 1).
			new ActionOption<GameState>("Invaders have -1 Health.", gs => {
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
			});

		static ActionOption<GameState> TakeBlightOn1LessDamage => new ActionOption<GameState>(
			"The land takes blight on 1 less Damage.", 
			gs => { gs.DamageToBlightLand = 1; }
		);

		static ActionOption<GameState> AddBlightDestroyesPresence => new ActionOption<GameState>(
			"When you add blight, it Destroys all presence/beast in that land and 1 presence (total) in an adjacent land.", 
			gs => {
				// !!! Implement this
			}
		);


	}

}
