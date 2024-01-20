namespace SpiritIsland.Tests;

/// <summary>
/// Initializes a GameState with spirit on BoardA
/// </summary>
public class BoardAGame {

	protected VirtualUser User {get;}

	protected Spirit _spirit;
	protected GameState _gameState;
	protected Board _board;

	public BoardAGame( Spirit spirit ){
		_spirit = spirit;
		User = new VirtualUser(spirit);
		_board = Board.BuildBoardA();
		_gameState = new GameState(spirit, _board );
		_gameState.Given_InitializedMinorDeck();
	}

}