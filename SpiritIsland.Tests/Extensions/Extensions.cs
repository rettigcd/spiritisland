using SpiritIsland.Log;

namespace SpiritIsland.Tests;

static internal class Extensions {

	public static async Task PlaceOn( this SpiritPresence presence, Space space, GameState gameState ) {
		await using ActionScope scope = await ActionScope.Start(ActionCategory.Default);
		await gameState.Tokens[space].Add( presence.Token, 1 );
	}

	#region Generating Explorer Action on a space

	static public InvaderCard BuildInvaderCard( this Space space ) {
		var terrain = new[] { Terrain.Wetland, Terrain.Sand, Terrain.Jungle, Terrain.Mountain }.First( space.Is );
		return terrain != Terrain.Ocean ? InvaderCard.Stage1( terrain ) : throw new ArgumentException( "Can't invade oceans" );
	}

	// !!! should these use slots or Engines?
	static public Task DoARavage( this SpaceState space ) 
		=> new RavageSlot().ActivateCard(  space.Space.BuildInvaderCard(), GameState.Current );
	static public Task DoARavage( this Space space, GameState gs ) 
		=> new RavageSlot().ActivateCard( space.BuildInvaderCard(), gs );

	static public Task DoABuild( this Space space, GameState gameState )
		=> new BuildSlot().ActivateCard( space.BuildInvaderCard(), gameState );
	static public Task DoAnExplore( this Space space, GameState gs ) 
		=> new ExploreSlot().ActivateCard( space.BuildInvaderCard(), gs );


	#endregion

	static public void SetRevealedCount( this IPresenceTrack sut, int value ) {
		while(sut.Revealed.Count() < value)
			sut.Reveal( sut.RevealOptions.Single() );
	}

	static public string InvaderSummary( this SpaceState dict ) {

		// !!! Deprecate this.  Use .Invaders (to get just the invaders) then .Summary
		static int Order_CitiesTownsExplorers( HumanToken invader )
			=> -(invader.FullHealth * 10 + invader.RemainingHealth);
		return dict.InvaderTokens()
			.OrderBy( Order_CitiesTownsExplorers )
			.Select( invader => dict[invader] + invader.ToString() )
			.Join( "," );
	}

	static public void InitTestInvaderDeck(this GameState gameState, params InvaderCard[] cards ) {
		gameState.InvaderDeck = new InvaderDeck( cards.ToList(), null );// Don't try to inspect unused!
	}

	static public string Msg( this ILogEntry logEntry ) => logEntry.Msg( LogLevel.Info );

	static public TargetSpaceCtx TargetSpace( this SelfCtx ctx, string spaceLabel )
		=> ctx.Target( ctx.GameState.Spaces_Unfiltered.Downgrade().First( s => s.Label == spaceLabel ) );

}
