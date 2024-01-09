namespace SpiritIsland.JaggedEarth;

public class AllThingsWeaken : BlightCard {

	public AllThingsWeaken():base("All Things Weaken", "Ongoing, starting next turn: Invaders and dahan have -1 Health (min.1). The land takes blight on 1 less Damage (normally 1). When you add blight, it Destroys all presence/beast in that land and 1 presence (total) in an adjacent land.", 3) {}

	public override IActOn<GameState> Immediately => 
		// Ongoing, starting next turn:
		Cmd.AtTheStartOfNextRound( Cmd.Multiple<GameState>(
			// Invaders and Dahan have -1 Health (min, 1).
			ReduceTokenHealthBy1,
			// The land takes blight on 1 less Damage (normally 1).
			TakeBlightOn1LessDamage,
			// When you add blight, it Destroys all presence/beast in that land and 1 presence (total) in an adjacent land.
			AddBlightDestroyesPresence
		));

	static BaseCmd<GameState> ReduceTokenHealthBy1 => 

		// Invaders and Dahan have -1 Health (min, 1).
		new BaseCmd<GameState>("Invaders have -1 Health.", async gs => {
			// change the defaults
			var defaults = gs.Tokens.TokenDefaults;
			foreach(var invaderClass in defaults.Keys.OfType<HumanTokenClass>().ToArray()){
				HumanToken current = defaults[invaderClass].AsHuman();
				defaults[invaderClass] = current.AddHealth(-1);
			}

			// replace/update existing tokens
			foreach(var ss in gs.Spaces) {
				var humanTokenTypes = ss.Humans()
					.Where(x=>x.FullHealth > 1)
					.OrderBy(x=>x.FullHealth)
					.ToArray(); // least health first
				foreach(HumanToken oldToken in humanTokenTypes)
					await ss.AdjustPropsForAll( oldToken )
						.WithHumanAsync( oldToken.AddHealth( -1 ) );
			}
		});

	static BaseCmd<GameState> TakeBlightOn1LessDamage => new BaseCmd<GameState>(
		"The land takes blight on 1 less Damage.", 
		gs => { gs.DamageToBlightLand = 1; }
	);

	static BaseCmd<GameState> AddBlightDestroyesPresence => new BaseCmd<GameState>(
		"When you add blight, it Destroys all presence/beast in that land and 1 presence (total) in an adjacent land.", 
		gs => gs.AddIslandMod( new DestroyerOfBeastsAndPresence() )
	);

}

class DestroyerOfBeastsAndPresence : BaseModEntity, IHandleTokenAddedAsync {
	public async Task HandleTokenAddedAsync( SpaceState to, ITokenAddedArgs args ) {
		if(args.Added != Token.Blight) return;

		// Destroy all presence in this land.
		var toDestroy = to.OfAnyTag(Token.Beast)
			.Union( to.OfTag( TokenCategory.Presence ) )
			.ToArray();
		foreach(IToken t in toDestroy)
			await to.Destroy( t, to[t] );
		ActionScope.Current.LogDebug( "All Things Weaken - destroyed all Presence/Beast on " + ((Space)args.To).Text );

		// Destroy 1 presence in adjacent land
		var options = to.Adjacent_Existing
			.SelectMany(
				adj => adj.OfTag(TokenCategory.Presence).On(adj.Space)
			)
			.ToArray();

		var decision = new A.SpaceToken( "Presence to destroy", options, Present.Always );
		var token = await to.Space.Boards[0].FindSpirit().SelectAsync(decision);

		if(token == null) return;
		await token.Space.Tokens.Destroy(token.Token, 1);
		ActionScope.Current.LogDebug( "All Things Weaken - destroyed neighbor presence " + token );

	}
}