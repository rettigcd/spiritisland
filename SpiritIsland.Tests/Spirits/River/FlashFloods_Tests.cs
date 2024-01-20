namespace SpiritIsland.Tests.Spirits.River;

[Collection("BaseGame Spirits")]
public class FlashFloods_Tests {

	GameState _gameState;
	PowerCard _card;

	// immutable
	readonly PowerCard flashFloodsCard = PowerCard.For(typeof(FlashFloods));
	readonly Spirit _spirit;

	public FlashFloods_Tests():base() {
		_spirit = new RiverSurges();
	}

	[Fact]
	public async Task FlashFloods_Inland() {

		_gameState = new GameState( _spirit, Board.BuildBoardA() );
		_gameState.Phase = Phase.Fast;


		//   And: a game on Board-A
		var board = Board.BuildBoardA();
		_gameState.Island = new Island( board );

		//   And: Presence on A2 (city/coastal)
		var presenceSpace = board[2];
		_spirit.Given_HasPresenceOn(presenceSpace);
		//   And: 1 of each type of Invaders in Inland space (A4)
		Space targetSpace = board[4];
		var counts = _gameState.Tokens[targetSpace];
		counts.AdjustDefault( Human.City, 1 );
		counts.AdjustDefault( Human.Town, 1 );
		counts.AdjustDefault( Human.Explorer, 1 );
		_gameState.Assert_Invaders( targetSpace, "1C@3,1T@2,1E@1" );

		//   And: Purchased FlashFloods
		_card = _spirit.Hand.Single( c => c.Name == FlashFloods.Name );
		_spirit.Energy = _card.Cost;
		_spirit.PlayCard( _card );
		Assert.Contains( _card, _spirit.GetAvailableActions( _card.DisplaySpeed ).OfType<PowerCard>().ToList() ); // is fast

		// When:
		await _card.ActivateAsync( _spirit ).AwaitUser( _spirit, user => {
			user.TargetsLand( FlashFloods.Name, "A4" );
			user.NextDecision .HasPrompt( "Damage (1 remaining)" ).HasOptions( "C@3,T@2,E@1" ).Choose( "E@1" );
		} ).ShouldComplete();

		// Then:
		_gameState.Assert_Invaders( targetSpace, "1C@3,1T@2" );
	}

	[Fact]
	public async Task FlashFloods_Costal() {
		// Given: River
		//   And: a game on Board-A
		var board = Board.BuildBoardA();
		_gameState = new GameState( _spirit, board ) {
			Phase = Phase.Fast
		};
		//   And: Presence on A2 (city/coastal)
		var presenceSpace = board[2];
		_spirit.Given_HasPresenceOn(presenceSpace);
		//   And: 1 of each type of Invaders in Costal space (A2)
		Space targetSpace = board[2];
		var grp = _gameState.Tokens[targetSpace];
		grp.AdjustDefault( Human.City, 1 );
		grp.AdjustDefault( Human.Town, 1 );
		grp.AdjustDefault( Human.Explorer, 1);
		_gameState.Assert_Invaders(targetSpace, "1C@3,1T@2,1E@1" );

		//   And: Purchased FlashFloods
		_card = _spirit.Hand.Single(c=>c.Name == FlashFloods.Name);
		_spirit.Energy = _card.Cost;
		_spirit.PlayCard( _card );
		Assert.Contains(_card,_spirit.GetAvailableActions(_card.DisplaySpeed).OfType<PowerCard>().ToList()); // is fast

		await _card.ActivateAsync( _spirit ).AwaitUser( _spirit, user => {
			//  Select: A2
			user.TargetsLand(FlashFloods.Name,"A2");

			// Then: can apply 2 points of damage
			user.NextDecision.HasPrompt( "Damage (2 remaining)" ).HasOptions( "C@3,T@2,E@1" ).Choose( "C@3" );
			user.NextDecision.HasPrompt( "Damage (1 remaining)" ).HasOptions( "C@2,T@2,E@1" ).Choose( "C@2" );
		} ).ShouldComplete();

		// Then
		_gameState.Assert_Invaders(targetSpace, "1C@1,1T@2,1E@1" );
	}

	[Fact]
	public void FlashFloods_Stats() {
		flashFloodsCard.Assert_CardStatus( 2, Phase.Fast, "sun water" );
	}

}




