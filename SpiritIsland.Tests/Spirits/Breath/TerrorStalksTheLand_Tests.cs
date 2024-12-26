using SpiritIsland.NatureIncarnate;

namespace SpiritIsland.Tests.Spirits.Breath; 

public class TerrorStalksTheLand_Tests {

	public TerrorStalksTheLand_Tests() {
		_spirit = new BreathOfDarknessDownYourSpine();
		_board = Boards.A;
		_gameState = new SoloGameState( _spirit, _board);
	}

	[Theory]
	[InlineData( "1E@1" )]
	[InlineData( "1T@2" )]
	[InlineData( "1C@3" )]
	public async Task DamageLoneInvader_AbductsIt( string invaderStr ) {
		var tokens = _board[8].ScopeSpace;
		_gameState.Initialize();
		tokens.Given_InitSummary(invaderStr);
		tokens.Summary.ShouldBe( invaderStr );
		//   And: SS on neighbor
		_board[7].ScopeSpace.Init( _spirit.Presence.Token, 2 );

		// When: damaging Invader
		await _spirit.When_ResolvingCard<RouseTheTreesAndStones>( u => {
			u.NextDecision.HasPrompt( "Rouse the Trees and Stones: Target Space" ).Choose( "A8" );
		} ).ShouldComplete( "Rouse the Tree and Stone" );

		// Then: Invader has been abducted.
		tokens.Summary.ShouldBe( "[none]" );
		//  And: Explorer is in Endless Dark
		EndlessDark.Space.ScopeSpace.Summary.ShouldBe( invaderStr );
	}


	// Empowered Can abduct once
	[Fact]
	public async Task EmpoweredIncarna_CanAbductOnce() {
		var space = _board[5].ScopeSpace;

		// Given: Incarna, Town, Explorer on the same space
		space.Given_InitSummary("1BoDDYS+,1E@1,1T@2");
		space.Summary.ShouldBe("1BoDDYS+,1E@1,1T@2");

		// When: doing fast
		_gameState.Phase = Phase.Fast;
		await _spirit.SelectAndResolveActions(_gameState).AwaitUser(user => {
			// Then: can abduct once
			user.NextDecision.HasPrompt("Select Fast to resolve").HasOptions("Abduct Explorer/Town,Done").ChooseFirst();
			user.NextDecision.HasPrompt("Select Invader to Abduct").HasOptions("E@1 on A5,T@2 on A5,Done").ChooseFirst();
			//  But: no more.  All done
		}).ShouldComplete("Fast acitons");
	}

	// Not-Empowered, Cannot abduct at all.
	[Fact]
	public async Task NotEmpoweredIncarna_NoAbduct() {
		var space = _board[5].ScopeSpace;

		// Given: Incarna, Town, Explorer on the same space
		space.Given_InitSummary("1BoDDYS-,1E@1,1T@2");
		space.Summary.ShouldBe("1BoDDYS-,1E@1,1T@2");

		// When: doing fast
		_gameState.Phase = Phase.Fast;
		// Then no abduct action presented
		await _spirit.SelectAndResolveActions(_gameState).ShouldComplete("Fast acitons");
	}


	readonly Spirit _spirit;
	readonly Board _board;
	readonly GameState _gameState;

}
