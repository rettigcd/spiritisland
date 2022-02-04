namespace SpiritIsland;

/// <summary>
/// Used for events / fear that say "for each board"
/// </summary>
public class BoardCtx : SelfCtx {
	public Board Board { get; }
	public BoardCtx( Spirit spirit, GameState gs, Board board ):base(spirit, gs, Cause.Fear) {
		Board = board;
	}

	public Task SelectActionOption( params IExecuteOn<BoardCtx>[] options ) => SelectActionOption( "Select Power Option", options );
	public Task SelectActionOption( string prompt, params IExecuteOn<BoardCtx>[] options )=> SelectAction_Inner( prompt, options, Present.AutoSelectSingle, this );
	public Task SelectAction_Optional( string prompt, params IExecuteOn<BoardCtx>[] options )=> SelectAction_Inner( prompt, options, Present.Done, this );

}
