using SpiritIsland.NatureIncarnate;

namespace SpiritIsland.Tests.Spirits.Breath; 

public class TerrorStalksTheLand_Tests {

	public TerrorStalksTheLand_Tests() {
		_spirit = new BreathOfDarknessDownYourSpine();
		_board = Boards.A;
		_ = new GameState( _spirit, _board);
	}

	[Fact]
	public async Task DamageLoanExplorer_AbductsIt() {
		var tokens = _board[8].Tokens;
		// Given: only 1 explorer on target space
		tokens.InitDefault(Human.Explorer,1);
		tokens.Summary.ShouldBe("1E@1");
		//   And: SS on neighbor
		_board[7].Tokens.Init(_spirit.Presence.Token,2);

		// When: damaging Invader
		await _spirit.When_ResolvingCard<RouseTheTreesAndStones>( u => {
			u.NextDecision.HasPrompt( "Rouse the Trees and Stones: Target Space" ).HasOptions( "A5,A7,A8,EndlessDark" ).Choose( "A8" );
			u.NextDecision.HasPrompt( "Damage (2 remaining)" ).HasOptions( "E@1" ).Choose( "E@1" );
			// u.NextDecision.HasPrompt( "a" ).HasOptions( "b" ).Choose( "c" );
			// u.NextDecision.HasPrompt( "a" ).HasOptions( "b" ).Choose( "c" );
		} ).ShouldComplete("Rouse the Tree and Stone");

		// Then: Explorer has been abducted.
		tokens.Summary.ShouldBe("[none]");
		//  And: Explorer is in Endless Dark
		EndlessDark.Space.Tokens.Summary.ShouldBe("1E@1");

	}

	[Fact]
	public async Task DamageLoanTown_AbductsIt() {
		var tokens = _board[8].Tokens;
		// Given: only 1 explorer on target space
		tokens.InitDefault( Human.Town, 1 );
		tokens.Summary.ShouldBe( "1T@2" );
		//   And: SS on neighbor
		_board[7].Tokens.Init( _spirit.Presence.Token, 2 );

		// When: damaging Invader
		await _spirit.When_ResolvingCard<RouseTheTreesAndStones>( u => {
			u.NextDecision.HasPrompt( "Rouse the Trees and Stones: Target Space" ).HasOptions( "A5,A7,A8,EndlessDark" ).Choose( "A8" );
			u.NextDecision.HasPrompt( "Damage (2 remaining)" ).HasOptions( "T@2" ).Choose( "T@2" );
			u.NextDecision.HasPrompt( "Damage (1 remaining)" ).HasOptions( "T@1" ).Choose( "T@1" );
			// u.NextDecision.HasPrompt( "a" ).HasOptions( "b" ).Choose( "c" );
			// u.NextDecision.HasPrompt( "a" ).HasOptions( "b" ).Choose( "c" );
		} ).ShouldComplete( "Rouse the Tree and Stone" );

		// Then: Explorer has been abducted.
		tokens.Summary.ShouldBe( "[none]" );
		//  And: Explorer is in Endless Dark
		EndlessDark.Space.Tokens.Summary.ShouldBe( "1T@2" );

	}


	readonly Spirit _spirit;
	readonly Board _board;

}
