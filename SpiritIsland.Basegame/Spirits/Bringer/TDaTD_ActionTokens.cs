namespace SpiritIsland.Basegame;

public class TDaTD_ActionTokens : SpaceState {

	readonly static public SpecialRule Rule = new( "TO DREAM A THOUSAND DEATHS", "Your Powers never cause Damage, nor can they Destroy anything other than your own Presence. When your Powers would Destroy Invaders, instead generate 0/2/5 Fear and Pushes Invaders" );

	public TDaTD_ActionTokens( SpaceState spaceState )
		: base( spaceState ) 
	{
	}

	public override async Task DestroySpace() {
		// Destroy Invaders
		await new InvaderBinding( this ).DestroyAll( Human.Invader );
		// Destroy Bringer's presence
		await Destroy( BringerPresence, this[BringerPresence] );
	}

	public override async Task<TokenRemovedArgs> Remove( IToken token, int count, RemoveReason reason = RemoveReason.Removed ) {
		if(reason != RemoveReason.Destroyed || token == BringerPresence)
			return await base.Remove( token, count, reason );

		if(token.Class.Category == TokenCategory.Invader)
			await DestroyNInvaders( token.AsHuman(), this[token] );

		return null; // nothing removed
	}

	public override async Task<int> DestroyNInvaders( HumanToken invaderToDestroy, int countToDestroy ) {
		countToDestroy = Math.Min( countToDestroy, this[invaderToDestroy] );
		for(int i = 0; i < countToDestroy; ++i)
			await Destroy1Invader( invaderToDestroy );
		return countToDestroy;
	}

	SpiritPresenceToken BringerPresence => _bringerPresence ??= ActionScope.Current.Owner.Presence.Token;
	SpiritPresenceToken _bringerPresence;

	async Task Destroy1Invader( HumanToken invaderToken ) {

		// for everything BUT normal invaders, we do nothing
		if(invaderToken.Class.Category != TokenCategory.Invader) return; 
		if(invaderToken.Class.Variant != TokenVariant.Default) return; 
		
		// Normal Invaders - Push

		// Replace destroyed invader with the dreaming (non-dream-damaged) version.
		var newToken = invaderToken
			.SwitchClass( ToggleDreaming( invaderToken.Class ) ) // make dreaming
			.AddDamage( 0, -invaderToken.DreamDamage ); // remove nightmare damage
		Adjust( invaderToken, -1 );
		Adjust( newToken, 1 );

		var gameState = GameState.Current;
		gameState.Log( new SpiritIsland.Log.Debug( "Dream 1000 deaths destroy." ) );

		// Record Here
		TDaTD_ActionTokens.RecordSpaceWithDreamers( this );

		// Add fear
		gameState.Fear.AddDirect( new FearArgs( newToken.Class.FearGeneratedWhenDestroyed ) { space = Space } );

		// Push towns and explorers
		if(newToken.Class != DreamingCity) {
			var options = Adjacent;
			Space destination = await ActionScope.Current.Owner.Gateway.Decision( Select.ASpace.PushToken( newToken, Space, options, Present.Always ) );
			await MoveTo( newToken, destination ); // there is no Push(Token), so this will have to do.
			RecordSpaceWithDreamers( destination.Tokens );
		}

	}

	public override HumanToken GetNewDamagedToken( HumanToken invaderToken, int availableDamage ) {
		// since we are doing dream-damage, record here
		RecordSpaceWithDreamers( this );
		return invaderToken.AddDamage( 0, availableDamage );
	}

	#region static Dreaming

	static public void RecordSpaceWithDreamers( SpaceState spaceState ) {
		var scope = ActionScope.Current;

		// if this is first time we have a space
		bool isFirstTime = !SpacesWithDreamers.HasValue;
		if(isFirstTime)
			scope.AtEndOfThisAction( CleanupDreamDamage );

		SpacesWithDreamers.Value.Add( spaceState );
	}

	#region static - restore invaders

	static public void CleanupDreamDamage( ActionScope actionScope ) { // ! this one is ok
		foreach(SpaceState spaceState in SpacesWithDreamers.Value) {
			RemoveDreamDamage( spaceState );
			WakeUpDreamers( spaceState );
		}
	}

	static void WakeUpDreamers( SpaceState spaceState ) {
		var dreamers = spaceState.OfCategory( TokenCategory.Invader )
			.Cast<HumanToken>()
			.Where( x => x.Class.Variant == TokenVariant.Dreaming )
			.ToArray();
		foreach(HumanToken dreamer in dreamers)
			spaceState.ReplaceAllWith( dreamer, ToggleDreamer( dreamer ) );
	}

	static void RemoveDreamDamage( SpaceState spaceState ) {
		var damagedInvaders = spaceState.OfTypeHuman()
			.Where( t => t.DreamDamage != 0 )
			.ToArray();
		foreach(var damagedInvader in damagedInvaders)
			spaceState.ReplaceAllWith(
				damagedInvader,
				damagedInvader.AddDamage( 0, -damagedInvader.DreamDamage ) // restored
			);
	}

	#endregion

	#region static DreamTokens

	static public readonly HumanTokenClass DreamingCity = new HumanTokenClass( "City_Dreaming", TokenCategory.Invader, 5, Img.City, 3, TokenVariant.Dreaming );
	static public readonly HumanTokenClass DreamingTown = new HumanTokenClass( "Town_Dreaming", TokenCategory.Invader, 2, Img.Town, 2, TokenVariant.Dreaming );
	static public readonly HumanTokenClass DreamingExplorer = new HumanTokenClass( "Explorer_Dreaming", TokenCategory.Invader, 0, Img.Explorer, 1, TokenVariant.Dreaming );

	static public HumanToken ToggleDreamer( HumanToken token ) => token.SwitchClass( ToggleDreaming( token.Class ) );

	static public HumanTokenClass ToggleDreaming( HumanTokenClass tokenClass ) {
		if(tokenClass.Category == TokenCategory.Invader) {
			if(tokenClass == Human.Explorer) return DreamingExplorer;
			if(tokenClass == Human.Town) return DreamingTown;
			if(tokenClass == Human.City) return DreamingCity;
			if(tokenClass == DreamingExplorer) return Human.Explorer;
			if(tokenClass == DreamingTown) return Human.Town;
			if(tokenClass == DreamingCity) return Human.City;
		}
		throw new ArgumentException( $"{tokenClass} is not explorer, town, or city." );
	}

	#endregion

	static readonly ActionScopeValue<HashSet<SpaceState>> SpacesWithDreamers = new( 
		"SpacesWithDreamers", 
		() => new HashSet<SpaceState>()
	);

	#endregion


}