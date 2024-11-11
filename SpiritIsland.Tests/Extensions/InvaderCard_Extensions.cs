namespace SpiritIsland.Tests;

static class InvaderCard_Extensions {

	internal static Task When_Ravaging( this InvaderCard invaderCard )
		=> new RavageEngine().ActivateCard( invaderCard, GameState.Current ).ShouldComplete( "Ravage" );

	internal static Task When_Building( this InvaderCard invaderCard )
		=> new BuildEngine().ActivateCard( invaderCard ).ShouldComplete( "Build" );

	internal static Task When_Exploring( this InvaderCard invaderCard )
		=> new ExploreEngine().ActivateCard( invaderCard, GameState.Current ).ShouldComplete( "Explore" );

	internal static Task When_InvokingLevel( this IFearCard card, int level, Action<VirtualUser> userActions = null ) {
		userActions ??= (_)=>{};
		return (level switch {
			1 => card.Level1( GameState.Current ),
			2 => card.Level2( GameState.Current ),
			3 => card.Level3( GameState.Current ),
			_ => throw new ArgumentOutOfRangeException( nameof( level ) )
		}).AwaitUser( userActions ).ShouldComplete($"{card.GetType().Name}-{level}");
	}

}
