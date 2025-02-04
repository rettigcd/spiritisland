namespace SpiritIsland.Tests;

static class InvaderCard_Extensions {

	internal static Task When_Ravaging( this InvaderCard invaderCard )
		=> new RavageEngine().ActivateCard( invaderCard ).ShouldComplete( "Ravage" );

	internal static Task When_Building( this InvaderCard invaderCard )
		=> new BuildEngine().ActivateCard( invaderCard ).ShouldComplete( "Build" );

	internal static Task When_Exploring( this InvaderCard invaderCard )
		=> new ExploreEngine().ActivateCard( invaderCard, GameState.Current ).ShouldComplete( "Explore" );

	internal static Task When_InvokingLevel( this IFearCard card, int level, Action<VirtualUser> userActions = null ) {
		userActions ??= (_)=>{};
		return card.ActAsync(level).AwaitUser(userActions).ShouldComplete($"{card.GetType().Name}-{level}");
	}

}
