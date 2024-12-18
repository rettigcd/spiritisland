namespace SpiritIsland;

/// <summary>
/// Used for events / fear that say "for each board"
/// Binds a board to its spirit
/// </summary>
public class BoardCtx( Board _board ) : IHaveASpirit {

	public Spirit Self { get; } = _board.FindSpirit();

	#region Parts from SelfCtx

	//  =========  Parts from SelfCtx  ==============

	public Task<T?> SelectAsync<T>( A.TypedDecision<T> originalDecision ) where T : class, IOption 
		=> Self.SelectAsync<T>(originalDecision);

	public TargetSpaceCtx Target( SpaceSpec space ) => Self.Target( space );
	public TargetSpaceCtx Target( Space space) => Self.Target(space);

	#endregion


	public Board Board { get; } = _board;

}
