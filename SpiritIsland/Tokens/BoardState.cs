namespace SpiritIsland;

public class BoardState {
	public Board Board { get; }
	readonly GameState gameState;

	public BoardState(Board board, GameState gameState) {
		this.Board = board;
		this.gameState = gameState;
	}
	public IEnumerable<SpaceState> Spaces => Board.Spaces.Select(s=>gameState.Tokens[s]);
}