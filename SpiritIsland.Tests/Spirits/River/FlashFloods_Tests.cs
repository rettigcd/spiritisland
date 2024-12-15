namespace SpiritIsland.Tests.Spirits.River;

[Collection("BaseGame Spirits")]
public class FlashFloods_Tests {

	GameState _gameState;
	PowerCard _card;

	// immutable
	readonly PowerCard flashFloodsCard = PowerCard.For(typeof(FlashFloods));
	readonly RiverSurges _spirit;

	public FlashFloods_Tests():base() {
		_spirit = new RiverSurges();
	}

	[Fact]
	public async Task FlashFloods_Inland() {

		_gameState = new SoloGameState( _spirit, Boards.A ) {
			Phase = Phase.Fast
		};


		//   And: a game on Board-A
		var board = Boards.A;
		_gameState.Island = new Island( board );

		//   And: Presence on A2 (city/coastal)
		var presenceSpace = board[2];
		_spirit.Given_IsOn(presenceSpace);
		//   And: 1 of each type of Invaders in Inland space (A4)
		SpaceSpec targetSpace = board[4];
		var counts = _gameState.Tokens[targetSpace];
		counts.AdjustDefault( Human.City, 1 );
		counts.AdjustDefault( Human.Town, 1 );
		counts.AdjustDefault( Human.Explorer, 1 );
		targetSpace.Assert_HasInvaders( "1C@3,1T@2,1E@1" );

		//   And: Purchased FlashFloods
		_card = _spirit.Hand.Single( c => c.Title == FlashFloods.Name );
		_spirit.Energy = _card.Cost;
		_spirit.PlayCard( _card );
		Assert.Contains( _card, _spirit.GetAvailableActions( _card.Speed ).OfType<PowerCard>().ToList() ); // is fast

		// When:
		await _card.ActivateAsync( _spirit ).AwaitUser( user => {
			user.TargetsLand( FlashFloods.Name, "A4" );
			user.NextDecision .HasPrompt( "Damage (1 remaining)" ).HasOptions( "C@3,T@2,E@1" ).Choose( "E@1" );
		} ).ShouldComplete();

		// Then:
		targetSpace.Assert_HasInvaders( "1C@3,1T@2" );
	}

	[Fact]
	public async Task FlashFloods_Costal() {
		// Given: River
		//   And: a game on Board-A
		var board = Boards.A;
		_gameState = new SoloGameState( _spirit, board ) {
			Phase = Phase.Fast
		};
		//   And: Presence on A2 (city/coastal)
		_spirit.Given_IsOn(board[2]);
		//   And: 1 of each type of Invaders in Costal space (A2)
		SpaceSpec targetSpace = board[2];
		var grp = _gameState.Tokens[targetSpace];
		grp.AdjustDefault( Human.City, 1 );
		grp.AdjustDefault( Human.Town, 1 );
		grp.AdjustDefault( Human.Explorer, 1);
		targetSpace.Assert_HasInvaders( "1C@3,1T@2,1E@1" );

		//   And: Purchased FlashFloods
		_card = _spirit.Hand.Single(c=>c.Title == FlashFloods.Name);
		_spirit.Energy = _card.Cost;
		_spirit.PlayCard( _card );
		Assert.Contains(_card,_spirit.GetAvailableActions(_card.Speed).OfType<PowerCard>().ToList()); // is fast

		await _card.ActivateAsync( _spirit ).AwaitUser( user => {
			//  Select: A2
			user.TargetsLand(FlashFloods.Name,"A2");

			// Then: can apply 2 points of damage
			user.NextDecision.HasPrompt( "Damage (2 remaining)" ).HasOptions( "C@3,T@2,E@1" ).Choose( "C@3" );
			user.NextDecision.HasPrompt( "Damage (1 remaining)" ).HasOptions( "C@2,T@2,E@1" ).Choose( "C@2" );
		} ).ShouldComplete();

		// Then
		targetSpace.Assert_HasInvaders( "1C@1,1T@2,1E@1" );
	}

	[Fact]
	public void FlashFloods_Stats() {
		flashFloodsCard.Assert_CardStatus( 2, Phase.Fast, "sun water" );
	}

}




