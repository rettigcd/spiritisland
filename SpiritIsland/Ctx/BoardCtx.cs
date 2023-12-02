namespace SpiritIsland;

/// <summary>
/// Used for events / fear that say "for each board"
/// Binds a board to its spirit
/// </summary>
public class BoardCtx : IHaveASpirit {

	// Binds Board to Spirit.

	#region constructors

	public BoardCtx( Board board ) {
		Board = board;
		Self = board.FindSpirit();
	}

	public Spirit Self { get; }

	#endregion

	#region Parts from SelfCtx

	//  =========  Parts from SelfCtx  ==============

	public Task<T> SelectAsync<T>( A.TypedDecision<T> originalDecision ) where T : class, IOption 
		=> Self.SelectAsync<T>( originalDecision );

	public TargetSpaceCtx Target( Space space ) => Self.Target( space );

	#endregion


	public Board Board { get; }

}
