namespace SpiritIsland.Tests;

static class InvaderCard_Extensions {

	internal static Task When_Ravaging( this InvaderCard invaderCard )
		=> new RavageEngine().ActivateCard( invaderCard, GameState.Current ).ShouldComplete( "Ravage" );

	internal static Task When_Building( this InvaderCard invaderCard )
		=> new BuildEngine().ActivateCard( invaderCard, GameState.Current ).ShouldComplete( "Build" );

	internal static Task When_Exploring( this InvaderCard invaderCard )
		=> new ExploreEngine().ActivateCard( invaderCard, GameState.Current ).ShouldComplete( "Explore" );

	internal static Task When_InvokingLevel( this IFearCard card, int level, Action userActions ) {
		var ctx = new GameCtx( GameState.Current );
		return (level switch {
			1 => card.Level1( ctx ),
			2 => card.Level2( ctx ),
			3 => card.Level3( ctx ),
			_ => throw new ArgumentOutOfRangeException( nameof( level ) )
		}).AwaitUserToComplete( $"{card.GetType().Name}-{level}", userActions );
	}


}
