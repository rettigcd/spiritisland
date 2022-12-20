namespace SpiritIsland.Tests.Spirits.Volcano; 

public class Volcano_Tests {

	[Trait( "Spirit", "SetupAction" )]
	[Fact]
	public void HasSetUp() {
		var fxt = new ConfigurableTestFixture { Spirit = new VolcanoLoomingHigh() };
		fxt.GameState.Initialize();
		fxt.Spirit.GetAvailableActions( Phase.Init ).Count().ShouldBe( 1 );
	}

	// COLLAPSE IN A BLAST OF LAVA AND STEAM
	// When your Presence is destroyed, in that land, deal 1 Damage per destroyed Presence to both Invaders and to Dahan.

	// VOLCANIC PEAKS TOWER OVER THE LANDSCAPE
	// Your Power Cards gain +1 Range if you have 3 or more Presence in the origin land.

}
