namespace SpiritIsland.Tests;

public class GrowthTests {

	protected VirtualUser User {get; }

	protected Spirit _spirit;
	protected GameState _gameState;
	protected Board _board;

	protected GrowthTests(Spirit spirit):base(){
		// PlayerState requires Spirit to be known because Spirit creates playerState.
		_spirit = spirit;
		User = new VirtualUser(spirit);
		_board = Board.BuildBoardA();
		_gameState = new GameState(spirit, _board );
		_gameState.Given_InitializedMinorDeck();
	}

	#region Board factories

	static protected Board BoardA => Board.BuildBoardA( GameBuilder.FourBoardLayout[0] );
	static protected Board BoardB => Board.BuildBoardB( GameBuilder.FourBoardLayout[1] );
	static protected Board BoardC => Board.BuildBoardC( GameBuilder.FourBoardLayout[2] );
	static protected Board BoardD => Board.BuildBoardD( GameBuilder.FourBoardLayout[3] );

	#endregion

}