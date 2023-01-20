namespace SpiritIsland.JaggedEarth;

public class AllThingsWeaken : BlightCardBase {

	public AllThingsWeaken():base("All Things Weaken", "Ongoing, starting next turn: Invaders and dahan have -1 Health (min.1). The land takes blight on 1 less Damage (normally 1). When you add blight, it Destroys all presence/beast in that land and 1 presence (total) in an adjacent land.", 3) {}

	public override DecisionOption<GameCtx> Immediately => 
		// Ongoing, starting next turn:
		Cmd.AtTheStartOfNextRound( Cmd.Multiple<GameState>(
			// Invaders and Dahan have -1 Health (min, 1).
			ReduceTokenHealthBy1,
			// The land takes blight on 1 less Damage (normally 1).
			TakeBlightOn1LessDamage,
			// When you add blight, it Destroys all presence/beast in that land and 1 presence (total) in an adjacent land.
			AddBlightDestroyesPresence
		));

	static DecisionOption<GameState> ReduceTokenHealthBy1 => 

		// Invaders and Dahan have -1 Health (min, 1).
		new DecisionOption<GameState>("Invaders have -1 Health.", gs => {
			// change the defaults
			var defaults = gs.Tokens.TokenDefaults;
			foreach(var invaderClass in defaults.Keys.ToArray()){
				var current = defaults[invaderClass];
				defaults[invaderClass] = current.AddHealth(-1);
			}

			// replace/update existing tokens
			foreach(var ss in gs.AllActiveSpaces) {
				var tokenTypes = ss.Keys.OfType<HealthToken>()
					.Where(x=>x.FullHealth > 1)
					.OrderBy(x=>x.FullHealth)
					.ToArray(); // least health first
				foreach(var oldToken in tokenTypes) {
					var newToken = oldToken.AddHealth(-1);
					ss.Adjust( newToken, ss[oldToken] );
					ss.Init( oldToken, 0 );
					// if (newToken.IsDestroyed) !!! destroy new tokens
				}
			}
		});

	static DecisionOption<GameState> TakeBlightOn1LessDamage => new DecisionOption<GameState>(
		"The land takes blight on 1 less Damage.", 
		gs => { gs.DamageToBlightLand = 1; }
	);

	static DecisionOption<GameState> AddBlightDestroyesPresence => new DecisionOption<GameState>(
		"When you add blight, it Destroys all presence/beast in that land and 1 presence (total) in an adjacent land.", 
		gs => {
			// !!! Implement this
		}
	);


}