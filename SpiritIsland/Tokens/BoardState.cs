namespace SpiritIsland;

// !!! Get rid of this.  It is pointless.
public class BoardState {

	public Board Board { get; }

	public BoardState(Board board) {
		this.Board = board;
	}
	public IEnumerable<SpaceState> Spaces => Board.Spaces.Upgrade();
}