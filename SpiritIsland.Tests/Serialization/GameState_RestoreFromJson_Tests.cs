namespace SpiritIsland.Tests.Serialization;

/// <summary>
/// GameState.ToJson/RestoreFromJson - the restore driver docs/GameSerialization-Roadmap.md calls for,
/// sequencing every already-covered section (Tokens, InvaderDeck, Fear, PowerCardDeck, BlightCard,
/// Spirits, hook action lists, ReminderCards) onto an already normally-constructed GameState in the
/// order that's actually safe.
/// </summary>
public class GameState_RestoreFromJson_Tests {

	[Fact]
	public async Task FullRoundTrip_PreservesMutatedStateAndAdversaryWiring() {
		// Sweden level 1 - same adversary as Adversary_Tests.ReplayingInit_ThenWipingTokens..., which
		// already proves the engine-wiring/board-mutation half of this in isolation. This test proves the
		// composed driver end-to-end: build normally, mutate like a real game would, serialize, build a
		// second normal instance, restore onto it, and confirm both the data and the live wiring survive.
		var config = new GameConfiguration { Adversary = new AdversaryConfig( "Sweden", 1 ), ShuffleNumber = 7 };

		GameState originalGs = config.ConfigBoards( "A" ).ConfigSpirits( RiverSurges.Name ).BuildGame();

		// Mutate ordinary per-turn state so the round trip isn't proving a no-op.
		await originalGs.TriggerTimePasses(); // RoundNumber 1 -> 2
		originalGs.Fear.Add( 3 );
		Spirit originalSpirit = originalGs.Spirits[0];
		originalSpirit.Energy += 5;
		originalSpirit.DiscardPile.Add( originalSpirit.Hand[0] );
		originalSpirit.Hand.RemoveAt( 0 );
		originalGs.MajorCards!.Flip( 1 ); // draw a Major card - shuffles/advances the deck's own state

		var originalCtx = new GameStateSerializationContext( originalGs );
		JsonObject json = originalGs.ToJson( originalCtx );

		// Captured *after* serializing (same reasoning as PowerCardDeck_Tests), so this doesn't change
		// what was saved - it's "what the next Major draw would have been" for the snapshotted state.
		string originalNextMajorCard = originalGs.MajorCards!.Flip( 1 )[0].Title;

		// "Restore": build a second, independent GameState the exact same way (same adversary/boards/
		// spirits/shuffle number) - this reproduces Sweden's Explore.Engine wiring and SwedenHeavyMining
		// island mod identically - then let RestoreFromJson make the JSON snapshot authoritative for
		// everything else.
		GameState restoredGs = config.ConfigBoards( "A" ).ConfigSpirits( RiverSurges.Name ).BuildGame();
		var ctx = new GameStateSerializationContext( restoredGs );
		restoredGs.RestoreFromJson( json, ctx );

		restoredGs.RoundNumber.ShouldBe( originalGs.RoundNumber );
		restoredGs.Fear.EarnedFear.ShouldBe( originalGs.Fear.EarnedFear );

		Spirit restoredSpirit = restoredGs.Spirits[0];
		restoredSpirit.Energy.ShouldBe( originalSpirit.Energy );
		restoredSpirit.Hand.Select( c => c.Title ).ShouldBe( originalSpirit.Hand.Select( c => c.Title ) );
		restoredSpirit.DiscardPile.Select( c => c.Title ).ShouldBe( originalSpirit.DiscardPile.Select( c => c.Title ) );
		restoredGs.MajorCards!.Flip( 1 )[0].Title.ShouldBe( originalNextMajorCard );

		// Engine wiring survived (same proof as ReplayingInit_ThenWipingTokens... - not double-built here,
		// confirmed live and working end-to-end).
		restoredGs.InvaderDeck.Explore.Engine.GetType().Name.ShouldBe( "SwedenExplorer" );

		// Board mutation Sweden's level 1 Init makes (adding SwedenHeavyMining) wasn't double-applied.
		int originalModCount = originalGs.Tokens[originalGs.Island.Boards[0][2]]
			.ModsOfType<SpiritIsland.Basegame.SwedenHeavyMining>().Count();
		originalModCount.ShouldBe( 1 ); // sanity check the mod is really there, not vacuously 0
		restoredGs.Tokens[restoredGs.Island.Boards[0][2]]
			.ModsOfType<SpiritIsland.Basegame.SwedenHeavyMining>().Count().ShouldBe( originalModCount );

		// The restored game keeps playing correctly afterward - wiring stays live, not just data.
		await restoredGs.TriggerTimePasses();
		restoredGs.RoundNumber.ShouldBe( originalGs.RoundNumber + 1 );
	}

}
