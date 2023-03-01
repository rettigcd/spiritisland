
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
		SpiritExtensions.Adjust( self.Presence, space, 2 );

		// When: a spirit power destroys presence
		await using ActionScope actionScope = await ActionScope.Start(ActionCategory.Spirit_Power);
		Task t = GrowthThroughSacrifice.ActAsync( self.BindMyPowers().TargetSpirit(self) );
		self.NextDecision().HasPrompt( "Select presence to destroy" ).HasOptions("A5").Choose("A5");
		// (!! this is kind of a crappy sequence for Growth-thru-sacrifice.  Can we clean up the wording / choice order?)
		self.NextDecision().HasPrompt( "Select location to Remove Blight OR Add Presence" ).HasOptions( "A5" ).Choose( "A5" );
		self.NextDecision().HasPrompt( "Select Power Option" ).HasOptions( "Remove 1 blight from one of your lands,Add 1 presence to one of your lands" ).Choose( "Remove 1 blight from one of your lands" );

		// Then: no disease was added
		space.Summary.ShouldBe("[none]");

		t.IsCompleted.ShouldBeTrue();
	}

	[Trait( "SpecialRule", "LingeringPestilence" )]
	[Fact]
	public void Ravage_GeneratesDisease() {

		Spirit self = new VengeanceAsABurningPlague();
		Board boardA = Board.BuildBoardA();
		GameState gameState = new GameState( self, boardA );

		// Given: a space with presence
		SpaceState space = gameState.Tokens[boardA[5]];
		SpiritExtensions.Adjust( self.Presence, space, 2 );
		//   And: invaders
		space.InitTokens("1C@3");
		//   And: island won't bight
		gameState.IslandWontBlight();

		// When: the city ravages
		space.Ravage().Wait();

		// Then: presence is destroyed
		space[self.Token].ShouldBe(1);

		//  And: Disease was added
		space.Disease.Count.ShouldBe(1);
	}

}
