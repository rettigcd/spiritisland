namespace SpiritIsland;

/// <summary>
/// Used for events / fear that say "for each board"
/// Binds a board to its spirit
/// </summary>
public class BoardCtx : SelfCtx {

	// Binds Board to Spirit.

	#region constructors

	public BoardCtx( Board board ) : base( board.FindSpirit() ) {
		Board = board;
	}

	#endregion

	public Board Board { get; }

}
