
namespace SpiritIsland.Tests.Spirits.Vengeance;

public class LingeringPestilence_Tests {

	[Trait("SpecialRule", "LingeringPestilence" )]
	[Fact]
	public async Task Powers_DoNotGenerateDisease() {

		Spirit self = new VengeanceAsABurningPlague();
		Board boardA = Boards.A;
		GameState gameState = new SoloGameState( self, boardA );

		// Given: a space with presence
		Space space = gameState.Tokens[boardA[5]];
		self.Given_IsOn( space, 2 );

		// When: a spirit power destroys presence
		await self.When_ResolvingCard<GrowthThroughSacrifice>( (user) => {
			user.NextDecision.HasPrompt( "Select presence to destroy" ).HasOptions( "VaaBP" ).Choose( "VaaBP" );
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

		var gs = new SoloGameState(new VengeanceAsABurningPlague());
		var boardA = gs.Board;

		// Given: a space with presence
		Space space = gs.Tokens[boardA[5]];
		gs.Spirit.Given_IsOn( space, 2 );
		//   And: invaders
		space.Given_HasTokens("1C@3");
		//   And: island won't bight
		gs.IslandWontBlight();

		// When: the city ravages
		await space.SpaceSpec.When_CardRavages();

		// Then: presence is destroyed
		gs.Spirit.Presence.CountOn( space ).ShouldBe(1);

		//  And: Disease was added
		space.Disease.Count.ShouldBe(1);
	}

}
