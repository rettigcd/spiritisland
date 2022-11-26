using SpiritIsland.JaggedEarth;

namespace SpiritIsland.Tests;

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
	public void Destroy2Fewer_Ravage(string startingTokens, string expectedEndingTokens ) {
		var fxt = new ConfigurableTestFixture();
		var space = fxt.Board[5];
		// Given: # of Dahan and Towns
		fxt.InitTokens( space, startingTokens);
		//   And: played ShareSecretsOfSurvival
		var ctx= fxt.SelfCtx.Target( space );
		ShareSecretsOfSurvival.Destroy2FewerDahan.Execute( ctx );

		//  When: ravage
		new RavageAction(fxt.GameState, fxt.GameState.Invaders.On(space,new UnitOfWork())).Exec().Wait();

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
	public void BirdsCryWarning_Ravage( string startingTokens, string expectedEndingTokens ) {


		var fxt = new ConfigurableTestFixture();
		var space = fxt.Board[5];
		// Given: # of Dahan and Towns
		fxt.InitTokens( space, startingTokens );
		//   And: played ShareSecretsOfSurvival
		var ctx = fxt.SelfCtx.Target( space );
		BirdsCryWarning.Destroy2FewerDahan.Execute( ctx );

		//  When: ravage
		new RavageAction( fxt.GameState, fxt.GameState.Invaders.On( space, new UnitOfWork() ) ).Exec().Wait();

		//  Then: expected dahan
		ctx.Tokens.Summary.ShouldBe( expectedEndingTokens );
	}


}
