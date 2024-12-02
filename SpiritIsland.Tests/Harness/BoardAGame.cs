namespace SpiritIsland.Tests;

/// <summary>
/// Initializes a GameState with spirit on BoardA
/// </summary>
public class BoardAGame {

	protected VirtualUser User {get;}

	protected Spirit _spirit;
	protected SoloGameState _gameState;
	protected Board _board;

	public BoardAGame( Spirit spirit ){
		_spirit = spirit;
		User = new VirtualUser(spirit);
		_gameState = new SoloGameState(spirit, Boards.A );
		_board = _gameState.Board;
		_gameState.Given_InitializedMinorDeck();
	}

}