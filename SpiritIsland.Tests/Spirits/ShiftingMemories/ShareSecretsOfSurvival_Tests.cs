namespace SpiritIsland.Tests;

[Trait("Feature","DestroyFewerDahan")]
public class ShareSecretsOfSurvival_Tests {

	[Theory]
	// more damage than dahan - makes sure saved dahan are not re-damaged by additional damage
	[InlineData( "10T@2", "1B,10T@2" )]
	[InlineData( "1D@2,10T@2", "1B,1D@2,9T@2" )]
	[InlineData( "2D@2,10T@2", "1B,2D@2,8T@2" )]
	[InlineData( "3D@2,10T@2", "1B,2D@2,8T@2" )]
	[InlineData( "4D@2,10T@2", "1B,2D@2,8T@2" )]
	// more dahan than damage - makes sure dahan count doesn't go down until we have more than 2 destroyed
	[InlineData( "9D@2", "9D@2" )]
	[InlineData( "9D@2,1T@2", "1B,9D@2" )]
	[InlineData( "9D@2,2T@2", "1B,9D@2" )]
	[InlineData( "9D@2,3T@2", "1B,8D@2" )]
	[InlineData( "9D@2,4T@2", "1B,7D@2" )]
	// Predamaged that get restored
	[InlineData( "2D@1,1T@2", "1B,2D@2" )]
	[InlineData( "1D@1,1D@2,1C@3,1E@1", "1B,2D@2" )]
	[InlineData( "3D@1,1T@2", "1B,1D@1,2D@2" )]
	[InlineData( "2D@2,1C@3,1T@2,1E@1", "1B,2D@2,1T@2" )]
	public void Destroy2Fewer_EachTime(string startingTokens, string expectedEndingTokens ) {
		var fxt = new ConfigurableTestFixture();
		var space = fxt.Board[5];
		// Given: # of Dahan and Towns
		fxt.InitTokens( space, startingTokens );
		//   And: Island won't blight
		fxt.GameState.IslandWontBlight();
		//   And: played ShareSecretsOfSurvival
		var ctx = fxt.SelfCtx.Target( space );
		Play_ShareSecretsOfSurvival( ctx );

		//  When: ravage
		fxt.GameState.Tokens[space].Ravage().Wait();

		//  Then: expected dahan
		ctx.Tokens.Summary.ShouldBe( expectedEndingTokens );
	}

	[Theory]
	// more damage than dahan - makes sure saved dahan are not re-damaged by additional damage
	[InlineData( "10T@2", "1B,10T@2" )]
	[InlineData( "1D@2,10T@2", "1B,1D@2,9T@2" )]
	[InlineData( "2D@2,10T@2", "1B,2D@2,8T@2" )]
	[InlineData( "3D@2,10T@2", "1B,2D@2,8T@2" )]
	[InlineData( "4D@2,10T@2", "1B,2D@2,8T@2" )]
	// more dahan than damage - makes sure dahan count doesn't go down until we have more than 2 destroyed
	[InlineData( "9D@2", "9D@2" )]
	[InlineData( "9D@2,1T@2", "1B,9D@2" )]
	[InlineData( "9D@2,2T@2", "1B,9D@2" )]
	[InlineData( "9D@2,3T@2", "1B,8D@2" )]
	[InlineData( "9D@2,4T@2", "1B,7D@2" )]
	// Predamaged that get restored
	[InlineData( "2D@1,1T@2", "1B,2D@2" )]
	[InlineData( "1D@1,1D@2,1C@3,1E@1", "1B,2D@2" )]
	[InlineData( "3D@1,1T@2", "1B,1D@1,2D@2" )]
	[InlineData( "2D@2,1C@3,1T@2,1E@1", "1B,2D@2,1T@2" )]
	public void Destroy2Fewer_NextTime( string startingTokens, string expectedEndingTokens ) {


		var fxt = new ConfigurableTestFixture();
		var space = fxt.Board[5];
		// Given: # of Dahan and Towns
		fxt.InitTokens( space, startingTokens );
		fxt.GameState.IslandWontBlight();
		//   And: played ShareSecretsOfSurvival
		var ctx = fxt.SelfCtx.Target( space );
		Play_ShareSecretsOfSurvival( ctx );

		//  When: ravage
		fxt.GameState.Tokens[space].Ravage().Wait();

		//  Then: expected dahan
		ctx.Tokens.Summary.ShouldBe( expectedEndingTokens );
	}

	static void Play_ShareSecretsOfSurvival( TargetSpaceCtx ctx ) {
		Task task = ShareSecretsOfSurvival.ActAsync( ctx );
		ctx.Self.NextDecision().HasPrompt( "Select Power Option" )
			.Choose( "Each time dahan would be destroyed in target land, Destroy 2 fewer dahan." );
		task.IsCompleted.ShouldBeTrue();
	}

}
