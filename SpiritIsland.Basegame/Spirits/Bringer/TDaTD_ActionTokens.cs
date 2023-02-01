namespace SpiritIsland.Basegame;

public class TDaTD_ActionTokens : ActionableSpaceState {

	readonly static public SpecialRule Rule = new( "TO DREAM A THOUSAND DEATHS", "Your Powers never cause Damage, nor can they Destroy anything other than your own Presence. When your Powers would Destroy Invaders, instead generate 0/2/5 Fear and Pushes Invaders" );

	readonly SelfCtx _selfCtx;

	public TDaTD_ActionTokens( SelfCtx selfCtx, Space space ) 
		: base( selfCtx.GameState.Tokens[space], selfCtx.ActionScope ) 
	{
		_selfCtx = selfCtx;
	}

	public override async Task<PublishTokenRemovedArgs> Remove( IToken token, int count, RemoveReason reason = RemoveReason.Removed ) {
		if(reason != RemoveReason.Destroyed)
			return await base.Remove( token, count, reason );

		count = Math.Min(count, this[token]);
		while(0<count--)
			await Destroy1Token( token );

		return null; // nothing removed
	}

	async Task Destroy1Token( IToken token ) {

		// for everything BUT normal invaders, we do nothing
		if(token.Class.Category != TokenCategory.Invader) return; 
		HumanToken invaderToken = (HumanToken)token;
		if(invaderToken.Class.Variant != TokenVariant.Default) return; 
		
		// Normal Invaders - Push

		// Replace destroyed invader with the dreaming (non-dream-damaged) version.
		var newToken = invaderToken
			.SwitchClass( BringerSpaceCtx.ToggleDreaming( invaderToken.Class ) ) // make dreaming
			.AddDamage( 0, -invaderToken.DreamDamage ); // remove nightmare damage
		Adjust( invaderToken, -1 );
		Adjust( newToken, 1 );

		var gameState = _selfCtx.GameState;
		gameState.Log( new SpiritIsland.Log.Debug( "Dream 1000 deaths destroy." ) );

		// Record Here
		RecordSpaceWithDreamers( this );

		// Add fear
		gameState.Fear.AddDirect( new FearArgs( newToken.Class.FearGeneratedWhenDestroyed ) { space = Space } );

		// Push towns and explorers
		if(newToken.Class != BringerSpaceCtx.DreamingCity) {
			var options = Adjacent.Where( gameState.Island.Terrain_ForPower.IsInPlay );
			Space destination = await _selfCtx.Decision( Select.Space.PushToken( newToken, Space, options, Present.Always ) );
			await MoveTo( newToken, destination ); // there is no Push(Token), so this will have to do.
			RecordSpaceWithDreamers( gameState.Tokens[destination] );
		}

	}

	public void RecordSpaceWithDreamers( SpaceState spaceState ) {
		var list = ActionScope.ContainsKey( SpacesWithDreamers )
			? (HashSet<SpaceState>)ActionScope[SpacesWithDreamers]
			: (HashSet<SpaceState>)(ActionScope[SpacesWithDreamers] = new HashSet<SpaceState>());
		list.Add( spaceState );
	}
	const string SpacesWithDreamers = "SpacesWithDreamers";


	public override HumanToken GetNewDamagedToken( HumanToken invaderToken, int availableDamage ) {
		// since we are doing dream-damage, record here
		RecordSpaceWithDreamers( this );
		return invaderToken.AddDamage( 0, availableDamage );
	}

	public override async Task<int> DestroyNTokens( HumanToken invaderToDestroy, int countToDestroy ) {
		countToDestroy = Math.Min( countToDestroy, this[invaderToDestroy] );
		for(int i = 0; i < countToDestroy; ++i)
			await Destroy1Token( invaderToDestroy );
		return countToDestroy;
	}

}