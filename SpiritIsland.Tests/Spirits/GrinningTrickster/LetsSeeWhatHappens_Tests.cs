namespace SpiritIsland.Tests.Spirits.GrinningTrickster;

public class LetsSeeWhatHappens_Tests : BoardAGame {

	public LetsSeeWhatHappens_Tests() : base( new GrinningTricksterStirsUpTrouble() ) {
		_gameState.Initialize();
	}

	[Fact]
	public async Task Level1_UsesTheDrawnMinorPowerAtMax() {
		// (used to throw "Spirit_Power category should not be used with Start")
		// Given: presence on a land with an explorer, and enough elements for the first tier
		foreach( Space s in _spirit.Presence.Lands.ToArray() )
			_spirit.Given_IsOn( s, 0 );
		Space a5 = _gameState.Tokens[_board[5]];
		_spirit.Given_IsOn( a5 );
		_spirit.Given_IsOn( _gameState.Tokens[_board[8]] ); // elsewhere too: the drawn card's blight may destroy the one on A5
		a5.Given_HasTokens( "1E@1" );
		_spirit.Elements.Add( ElementStrings.Parse( "1 moon,1 fire,2 air" ) );

		// When: the innate is used on that land
		await _spirit.When_ResolvingInnate( "Let's See What Happens", u => {
			u.NextDecision.HasPrompt( "Let's See What Happens: Target Space" ).Choose( "A5" );
			// Then: the drawn card is shown, and used at max
			u.NextDecision.HasPrompt( "Perform All Action at Max" ).Choose( "Drought $1 (Slow)" );
		} );
	}
}
