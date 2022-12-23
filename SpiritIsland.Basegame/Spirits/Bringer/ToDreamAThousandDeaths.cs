namespace SpiritIsland.Basegame;

public class ToDreamAThousandDeaths : InvaderBinding {

	readonly static public SpecialRule Rule = new( "TO DREAM A THOUSAND DEATHS", "Your Powers never cause Damage, nor can they Destroy anything other than your own Presence. When your Powers would Destroy Invaders, instead generate 0/2/5 Fear and Pushes Invaders" );

	#region static - restore invaders

	static public void CleanupDreamDamage( UnitOfWork uow ) {
		if(!uow.ContainsKey( SpacesWithDreamers )) return;

		foreach( var spaceState in (IEnumerable<SpaceState>)uow[SpacesWithDreamers]) {
			//gs.Log( new LogDebug( $"Waking up: {dreamingInvaders.Select( i => i.SpaceAbreviation ).Join( "," )} on {spaceState.Space.Text}." ) );
			RemoveDreamDamage( spaceState );
			WakeUpDreamers( spaceState );
		}
	}

	static void WakeUpDreamers( SpaceState spaceState ) {
		var dreamers = spaceState.OfAnyHealthClass( DreamInvaders ).ToArray();
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

	static public readonly HealthTokenClass DreamingCity = new HealthTokenClass( "City_Dreaming", 3, TokenCategory.Invader, 5, Img.City, 3 );
	static public readonly HealthTokenClass DreamingTown = new HealthTokenClass( "Town_Dreaming", 2, TokenCategory.Invader, 2, Img.Town, 2 );
	static public readonly HealthTokenClass DreamingExplorer = new HealthTokenClass( "Explorer_Dreaming", 1, TokenCategory.Invader, 0, Img.Explorer, 1 );
	static public readonly HealthTokenClass[] DreamInvaders = new HealthTokenClass[] { DreamingCity, DreamingTown, DreamingExplorer };

	static public HealthToken ToggleDreamer( HealthToken token ) => token.SwitchClass( ToggleDreaming( token.Class ) );

	static HealthTokenClass ToggleDreaming( HealthTokenClass tokenClass ) {
		if( tokenClass.Category == TokenCategory.Invader) {
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

	#region constructor

	public ToDreamAThousandDeaths( BringerSpaceCtx ctx )
		: base( ctx.GameState, ctx.Tokens, ctx.CurrentActionId ) {
		this.ctx = ctx;
	}

	#endregion constructor

	protected override HealthToken GetNewDamagedToken( HealthToken invaderToken, int availableDamage ) {

		// since we are doing dream-damage, record here
		RecordSpaceWithDreamers( ctx.Tokens );

		return invaderToken.AddDamage( 0, availableDamage );
	}

	protected override async Task Destroy1( HealthToken original, bool _ ) {

		// Replace destroyed invader with the dreaming (non-dream-damaged) version.
		var newToken = original
			.SwitchClass( ToggleDreaming( original.Class ) ) // make dreaming
			.AddDamage( 0, -original.DreamDamage ); // remove nightmare damage
		ctx.Tokens.Adjust( original, -1 );
		ctx.Tokens.Adjust( newToken, 1 );

		ctx.GameState.Log( new LogDebug( "Dream 1000 deaths destroy." ) );

		// Record Here
		RecordSpaceWithDreamers( ctx.Tokens );

		// Add fear
		ctx.GameState.Fear.AddDirect( new FearArgs {
			space = ctx.Space,
			count = newToken.Class.FearGeneratedWhenDestroyed,
			FromDestroyedInvaders = false // false => Dread Apparitions counts this for defence.
		} );

		// Push towns and explorers
		if(newToken.Class != DreamingCity)
			await PushDestroyedInvader( newToken );

	}

	#region private

	async Task PushDestroyedInvader( HealthToken invader ) {

		var destination = await ctx.Decision( Select.Space.PushToken( invader, ctx.Space, ctx.Tokens.Adjacent.Where( ctx.TerrainMapper.IsInPlay ), Present.Always ) );
		await ctx.Tokens.MoveTo( invader, destination, ctx.CurrentActionId ); // there is no Push(Token), so this will have to do.
		RecordSpaceWithDreamers( ctx.GameState.Tokens[destination] );
	}

	void RecordSpaceWithDreamers( SpaceState spaceState ) {
		var list = UnitOfWork.ContainsKey( SpacesWithDreamers )
			? (HashSet<SpaceState>)UnitOfWork[SpacesWithDreamers]
			: (HashSet<SpaceState>)(UnitOfWork[SpacesWithDreamers] = new HashSet<SpaceState>());
		list.Add( spaceState );
	}

	const string SpacesWithDreamers = "SpacesWithDreamers";

	readonly BringerSpaceCtx ctx;

	#endregion
}