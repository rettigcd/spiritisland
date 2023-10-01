
namespace SpiritIsland.Tests.Spirits.Vengeance;

public class LingeringPestilence_Tests {

	[Trait("SpecialRule", "LingeringPestilence" )]
	[Fact]
	public async Task Powers_DoNotGenerateDisease() {

		Spirit self = new VengeanceAsABurningPlague();
		Board boardA = Board.BuildBoardA();
		GameState gameState = new GameState( self, boardA );

		// Given: a space with presence
		SpaceState space = gameState.Tokens[boardA[5]];
		SpiritExtensions.Given_Adjust( self.Presence, space, 2 );

		// When: a spirit power destroys presence
		await self.When_ResolvingCard<GrowthThroughSacrifice>( (user) => {
			user.NextDecision.HasPrompt( "Select presence to destroy" ).HasOptions( "VaaBP on A5" ).Choose( "VaaBP on A5" );
			// (!! this is kind of a crappy sequence for Growth-thru-sacrifice.  Can we clean up the wording / choice order?)
			user.NextDecision.HasPrompt( "Select location to Remove Blight OR Add Presence" ).HasOptions( "A5" ).Choose( "A5" );
			user.NextDecision.HasPrompt( "Select Power Option" ).HasOptions( "Remove 1 blight from one of your lands,Add 1 presence to one of your lands" ).Choose( "Remove 1 blight from one of your lands" );
		} );

		// Then: no disease was added
		space.Summary.ShouldBe("1VaaBP");
	}

	[Trait( "SpecialRule", "LingeringPestilence" )]
	[Fact]
	public async Task Ravage_GeneratesDisease() {

		Spirit self = new VengeanceAsABurningPlague();
		Board boardA = Board.BuildBoardA();
		GameState gameState = new GameState( self, boardA );

		// Given: a space with presence
		SpaceState space = gameState.Tokens[boardA[5]];
		SpiritExtensions.Given_Adjust( self.Presence, space, 2 );
		//   And: invaders
		space.Given_HasTokens("1C@3");
		//   And: island won't bight
		gameState.IslandWontBlight();

		// When: the city ravages
		await space.Space.When_Ravaging();

		// Then: presence is destroyed
		self.Presence.CountOn( space ).ShouldBe(1);

		//  And: Disease was added
		space.Disease.Count.ShouldBe(1);
	}

}
