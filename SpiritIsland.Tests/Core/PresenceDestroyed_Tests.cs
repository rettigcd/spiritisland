namespace SpiritIsland.Tests.Core;

public class PresenceDestroyed_Tests {

	[Fact]
	public async Task DestroyingPresence_IsLogged_WithSpiritLandAndCount() {
		// Given: a spirit with Presence on a land
		var spirit = new LightningsSwiftStrike();
		var gameState = new SoloGameState( spirit, Boards.A );
		Space space = ActionScope.Current.Spaces.Single( x => x.Label == "A5" );
		spirit.Given_IsOn( space );
		spirit.Given_IsOn( ActionScope.Current.Spaces.Single( x => x.Label == "A6" ) ); // a 2nd presence, so destroying one does not lose the game
		var log = new List<Log.ILogEntry>();
		gameState.NewLogEntry += log.Add;
		await using ActionScope actionScope = await ActionScope.StartSpiritAction( ActionCategory.Spirit_Power, spirit );

		// When: that Presence is destroyed
		await space.Destroy( spirit.Presence.Token, 1 );

		// Then: a Presence-destroyed entry names the spirit, the land and how many
		string msg = log.OfType<Log.PresenceDestroyed>().Single().Msg( Log.LogLevel.Info );
		msg.ShouldContain( spirit.SpiritName );
		msg.ShouldContain( "A5" );
		msg.ShouldContain( "1 Presence destroyed" );
		spirit.Presence.Destroyed.Count.ShouldBe( 1 );
	}

	[Fact]
	public async Task RemovingOtherTokens_DoesNotLogPresenceDestroyed() {
		var spirit = new LightningsSwiftStrike();
		var gameState = new SoloGameState( spirit, Boards.A );
		Space space = ActionScope.Current.Spaces.Single( x => x.Label == "A5" );
		spirit.Given_IsOn( space );
		space.Beasts.Init( 1 );
		var log = new List<Log.ILogEntry>();
		gameState.NewLogEntry += log.Add;
		await using ActionScope actionScope = await ActionScope.StartSpiritAction( ActionCategory.Spirit_Power, spirit );

		await space.Destroy( Token.Beast, 1 );

		log.OfType<Log.PresenceDestroyed>().ShouldBeEmpty();
	}
}
