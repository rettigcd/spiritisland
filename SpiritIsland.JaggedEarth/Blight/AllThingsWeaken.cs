namespace SpiritIsland.JaggedEarth;

public class AllThingsWeaken : BlightCard {

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
		new DecisionOption<GameState>("Invaders have -1 Health.", async gs => {
			// change the defaults
			var defaults = gs.Tokens.TokenDefaults;
			foreach(var invaderClass in defaults.Keys.OfType<HumanTokenClass>().ToArray()){
				HumanToken current = defaults[invaderClass].AsHuman();
				defaults[invaderClass] = current.AddHealth(-1);
			}

			// replace/update existing tokens
			foreach(var ss in gs.Spaces) {
				var humanTokenTypes = ss.Keys.OfType<HumanToken>()
					.Where(x=>x.FullHealth > 1)
					.OrderBy(x=>x.FullHealth)
					.ToArray(); // least health first
				foreach(HumanToken oldToken in humanTokenTypes) {
					var newToken = oldToken.AddHealth(-1);
					ss.Init( oldToken, 0 );
					if(newToken.IsDestroyed) {
						await ss.Destroy(oldToken, ss[oldToken] );
					} else {
						ss.Init( oldToken, 0 );
						ss.Adjust( newToken, ss[oldToken] );
					}
				}
			}
		});

	static DecisionOption<GameState> TakeBlightOn1LessDamage => new DecisionOption<GameState>(
		"The land takes blight on 1 less Damage.", 
		gs => { gs.DamageToBlightLand = 1; }
	);

	static DecisionOption<GameState> AddBlightDestroyesPresence => new DecisionOption<GameState>(
		"When you add blight, it Destroys all presence/beast in that land and 1 presence (total) in an adjacent land.", 
		gs => gs.AddIslandMod( new DestroyerOfBeastsAndPresence() )
	);

}

class DestroyerOfBeastsAndPresence : BaseModEntity, IHandleTokenAddedAsync {
	public async Task HandleTokenAddedAsync( ITokenAddedArgs args ) {
		if(args.Added != Token.Blight) return;

		// Destroy all presence in this land.
		var toDestroy = args.To.OfAnyClass(Token.Beast)
			.Union( args.To.OfCategory(TokenCategory.Presence) )
			.Cast<IToken>()
			.ToArray();
		foreach(var t in toDestroy)
			await args.To.Destroy( t, args.To[t] );
		GameState.Current.LogDebug( "All Things Weaken - destroyed all Presence/Beast on " + args.To.Space.Text );

		// Destroy 1 presence in adjacent land
		var options = args.To.Adjacent_Existing
			.SelectMany(
				adj => adj.OfCategory(TokenCategory.Presence).Cast<IToken>()
					.Select(t => new SpaceToken(adj.Space,t) )
			)
			.ToArray();

		var decision = new Select.TokenFromManySpaces( "Presence to destroy", options, Present.Always );
		var token = await args.To.Space.Board.FindSpirit().Gateway
			.Decision(decision);

		if(token == null) return;
		await token.Space.Tokens.Destroy(token.Token, 1);
		GameState.Current.LogDebug( "All Things Weaken - destroyed neighbor presence " + token );

	}
}