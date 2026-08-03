using SpiritIsland.JaggedEarth;

namespace SpiritIsland.Tests.Adversaries;

public class HabsburgMonarchy_Tests {

	/// <summary>
	/// Level 5's Habsburg Reminder Card isn't modeled as a real InvaderCard - HabsburgMonarchy.InitFunc
	/// instead records the Code of the card sitting where the reminder card would be (UnrevealedCards[4])
	/// and registers a HabsburgReminderCardTrigger (IRunAfterInvaderPhase, internal) that fires once that
	/// Code shows up in the Build slot - same Build-slot-check pattern as CommandBeasts, and for the same
	/// reason (avoids an event subscription on a live InvaderCard). HabsburgReminderCardTrigger itself is
	/// internal with no InternalsVisibleTo to this project, so this drives it through the public
	/// PostInvaderPhaseActionRegistry tag dispatch rather than referencing the type directly.
	/// </summary>
	[Fact]
	public async Task Level5_ReminderCardTrigger_AddsCityAndTownsWhenItsCodeReachesBuild() {
		var config = new GameConfiguration { Adversary = new AdversaryConfig( HabsburgMonarchy.Name, 5 ), ShuffleNumber = 1 };
		GameState gs = config.ConfigBoards( "A" ).ConfigSpirits( RiverSurges.Name ).BuildGame();
		var ctx = new GameStateSerializationContext( gs );

		JsonArray json = new JsonArray( "HabsburgReminderCardTrigger", "J" );
		IRunAfterInvaderPhase restored = PostInvaderPhaseActionRegistry.Deserialize( json, ctx );

		Board board = gs.Island.Boards[0];
		int cityCountBefore = board.Spaces.ScopeTokens().Sum( s => s.Sum( Human.City ) );
		int townCountBefore = board.Spaces.ScopeTokens().Sum( s => s.Sum( Human.Town ) );

		// Build slot now holds a card whose Code matches the recorded trigger code - as if that card
		// just advanced off of Explore into Build.
		gs.InvaderDeck.Build.Cards.Add( InvaderCard.Stage1( Terrain.Jungle ) ); // Code "J"
		await restored.AfterInvaderPhase( gs );

		restored.RemoveAfterRun.ShouldBeTrue();

		int cityCountAfter = board.Spaces.ScopeTokens().Sum( s => s.Sum( Human.City ) );
		int townCountAfter = board.Spaces.ScopeTokens().Sum( s => s.Sum( Human.Town ) );
		( cityCountAfter + townCountAfter ).ShouldBeGreaterThan( cityCountBefore + townCountBefore );
	}

}
