using SpiritIsland.NatureIncarnate;

namespace SpiritIsland.Tests.Spirits.Breath; 

public class TerrorStalksTheLand_Tests {

	public TerrorStalksTheLand_Tests() {
		_spirit = new BreathOfDarknessDownYourSpine();
		_board = Boards.A;
		_ = new GameState( _spirit, _board);
	}

	[Theory]
	[InlineData( "1E@1" )]
	[InlineData( "1T@2" )]
	[InlineData( "1C@3" )]
	public async Task DamageLoneInvader_AbductsIt( string invaderStr ) {
		var tokens = _board[8].ScopeSpace;
		// Given: only 1 invader on target space
		tokens.InitDefault(invaderStr switch {
			"1E@1" => Human.Explorer,
			"1T@2" => Human.Town,
			"1C@3" => Human.City,
			_ => throw new ArgumentException($"invalid invader string '{invaderStr}'", nameof(invaderStr))
		}, 1 );
		tokens.Summary.ShouldBe( invaderStr );
		//   And: SS on neighbor
		_board[7].ScopeSpace.Init( _spirit.Presence.Token, 2 );

		// When: damaging Invader
		await _spirit.When_ResolvingCard<RouseTheTreesAndStones>( u => {
			u.NextDecision.HasPrompt( "Rouse the Trees and Stones: Target Space" ).HasOptions( "A5,A7,A8,Endless Dark" ).Choose( "A8" );
		} ).ShouldComplete( "Rouse the Tree and Stone" );

		// Then: Invader has been abducted.
		tokens.Summary.ShouldBe( "[none]" );
		//  And: Explorer is in Endless Dark
		EndlessDark.Space.ScopeSpace.Summary.ShouldBe( invaderStr );
	}


	readonly Spirit _spirit;
	readonly Board _board;

}
