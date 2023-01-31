namespace SpiritIsland.Basegame;

public class BringerSpaceCtx : TargetSpaceCtx {

	public BringerSpaceCtx(BringerCtx ctx,Space space ) : base( ctx, space ) { }

	// This does not derive from BringerCtx but from TargetSpaceCtx, so we need to override it here also.
	public override ActionableSpaceState TokensOn( Space space ) => new TDaTD_ActionTokens( this, space );


	public async Task<int> Destroy1TokenEx( HealthToken original ) {
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
			var destination = await Decision( Select.Space.PushToken( newToken, Space, Tokens.Adjacent.Where( TerrainMapper.IsInPlay ), Present.Always ) );
			await Tokens.MoveTo( newToken, destination ); // there is no Push(Token), so this will have to do.
			RecordSpaceWithDreamers( GameState.Tokens[destination] );
		}

		return 0;
	}

	public void RecordSpaceWithDreamers( SpaceState spaceState ) {
		var list = ActionScope.ContainsKey( SpacesWithDreamers )
			? (HashSet<SpaceState>)ActionScope[SpacesWithDreamers]
			: (HashSet<SpaceState>)(ActionScope[SpacesWithDreamers] = new HashSet<SpaceState>());
		list.Add( spaceState );
	}

	#region static - restore invaders

	static public void CleanupDreamDamage( UnitOfWork actionScope ) {
		if(!actionScope.ContainsKey( SpacesWithDreamers )) return;

		foreach(var spaceState in (IEnumerable<SpaceState>)actionScope[SpacesWithDreamers]) {
			RemoveDreamDamage( spaceState );
			WakeUpDreamers( spaceState );
		}
	}

	static void WakeUpDreamers( SpaceState spaceState ) {
		var dreamers = spaceState.OfCategory( TokenCategory.Invader )
			.Cast<HealthToken>()
			.Where( x => x.Class.Variant == TokenVariant.Dreaming )
			.ToArray();
		foreach(HealthToken dreamer in dreamers)
			spaceState.ReplaceAllWith( dreamer, ToggleDreamer( dreamer ) );
	}

	static void RemoveDreamDamage( SpaceState spaceState ) {
		var damagedInvaders = spaceState.Keys.OfType<HealthToken>()
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

	static public readonly HealthTokenClass DreamingCity = new HealthTokenClass( "City_Dreaming", TokenCategory.Invader, 5, Img.City, 3, TokenVariant.Dreaming );
	static public readonly HealthTokenClass DreamingTown = new HealthTokenClass( "Town_Dreaming", TokenCategory.Invader, 2, Img.Town, 2, TokenVariant.Dreaming );
	static public readonly HealthTokenClass DreamingExplorer = new HealthTokenClass( "Explorer_Dreaming", TokenCategory.Invader, 0, Img.Explorer, 1, TokenVariant.Dreaming );

	static public HealthToken ToggleDreamer( HealthToken token ) => token.SwitchClass( ToggleDreaming( token.Class ) );

	static public HealthTokenClass ToggleDreaming( HealthTokenClass tokenClass ) {
		if(tokenClass.Category == TokenCategory.Invader) {
			if(tokenClass == Invader.Explorer) return DreamingExplorer;
			if(tokenClass == Invader.Town) return DreamingTown;
			if(tokenClass == Invader.City) return DreamingCity;
			if(tokenClass == DreamingExplorer) return Invader.Explorer;
			if(tokenClass == DreamingTown) return Invader.Town;
			if(tokenClass == DreamingCity) return Invader.City;
		}
		throw new ArgumentException( $"{tokenClass} is not explorer, town, or city." );
	}

	#endregion

	const string SpacesWithDreamers = "SpacesWithDreamers";

}
