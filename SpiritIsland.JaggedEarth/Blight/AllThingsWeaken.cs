namespace SpiritIsland.JaggedEarth;

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
		new ActionOption<GameState>("Invaders have -1 Health.", async gs => {

			// change the defaults
			var defaults = gs.Tokens.TokenDefaults;
			foreach(var invaderClass in defaults.Keys.ToArray()){
				var current = defaults[invaderClass];
				if(current.FullHealth > 1)
					defaults[invaderClass] = current.AddHealth(-1);
			}

			// replace/update existing tokens
			foreach(var space in gs.Island.AllSpaces) {
				var tokens = gs.Tokens[space];
				var tokenTypes = tokens.Keys.OfType<HealthToken>()
					.Where(x=>x.FullHealth > 1)
					.OrderBy(x=>x.FullHealth)
					.ToArray(); // least health first
				foreach(var oldToken in tokenTypes) {
					var newToken = oldToken.AddHealth(-1);
					tokens.Adjust( newToken, tokens[oldToken] );
					tokens.Init( oldToken, 0 );
					// if (newToken.IsDestroyed) !!! destroy new tokens
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