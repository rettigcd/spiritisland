

namespace SpiritIsland.Basegame;

public class BringerSpaceCtx : TargetSpaceCtx {

	public BringerSpaceCtx(BringerCtx ctx,Space space ) : base( ctx, space ) { }

	// This does not derive from BringerCtx but from TargetSpaceCtx, so we need to override it here also.
	public override SpaceState TokensOn( Space space ) => new TDaTD_ActionTokens( this, space );


	public async Task<int> Destroy1TokenEx( HumanToken original ) {
		var tc = original.Class;
		if( tc.Category != TokenCategory.Invader || tc.Variant != TokenVariant.Default ) return 0; // only push normal invaders

		// Replace destroyed invader with the dreaming (non-dream-damaged) version.
		var newToken = original
			.SwitchClass( BringerSpaceCtx.ToggleDreaming( original.Class ) ) // make dreaming
			.AddDamage( 0, -original.DreamDamage ); // remove nightmare damage
		Tokens.Adjust( original, -1 );
		Tokens.Adjust( newToken, 1 );

		GameState.Log( new SpiritIsland.Log.Debug( "Dream 1000 deaths destroy." ) );

		// Record Here
		RecordSpaceWithDreamers( Tokens );

		// Add fear
		GameState.Fear.AddDirect( new FearArgs( newToken.Class.FearGeneratedWhenDestroyed ) { space = Space } );

		// Push towns and explorers
		if(newToken.Class != BringerSpaceCtx.DreamingCity) {
			var destination = await Decision( Select.ASpace.PushToken( newToken, Space, Tokens.Adjacent, Present.Always ) );
			await Tokens.MoveTo( newToken, destination ); // there is no Push(Token), so this will have to do.
			RecordSpaceWithDreamers( GameState.Tokens[destination] );
		}

		return 0;
	}

	static public void RecordSpaceWithDreamers( SpaceState spaceState ) {
		var scope = ActionScope.Current;

		// if this is first time we have a space
		bool isFirstTime = !scope.ContainsKey( SpacesWithDreamers );
		if( isFirstTime)
			scope.AtEndOfThisAction( CleanupDreamDamage );

		scope.SafeGet( SpacesWithDreamers, ()=> new HashSet<SpaceState>() )
			.Add( spaceState );
	}

	#region static - restore invaders

	static public void CleanupDreamDamage(ActionScope actionScope) { // ! this one is ok
		var spaces = actionScope.SafeGet( SpacesWithDreamers, Enumerable.Empty<SpaceState>() );
		foreach(SpaceState spaceState in spaces) {
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
		var damagedInvaders = spaceState.Keys.OfType<HumanToken>()
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

	const string SpacesWithDreamers = "SpacesWithDreamers";

}
