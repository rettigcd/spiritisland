namespace SpiritIsland;

/// <summary>
/// Used for events / fear that say "for each board"
/// </summary>
public class BoardCtx : SelfCtx {

	static public Spirit FindSpirit( GameState gameState, int boardIndex)
		=> gameState.Spirits[boardIndex < gameState.Spirits.Length ? boardIndex : 0];

	static public Spirit FindSpirit( GameState gameState, Board board ) {
		int index = 0;
		foreach(var b in gameState.Island.Boards) {
			if(b == board) {
				return gameState.Spirits[index];
			}
			++index;
		}
		return gameState.Spirits[0];
	}

	public Board Board { get; }

	public BoardCtx( Spirit spirit, GameState gs, Board board, UnitOfWork actionScope )
		:base(spirit, gs, actionScope ) {
		Board = board;
	}
	public BoardCtx( GameState gs, Board board, UnitOfWork actionScope )
		: base( FindSpirit(gs,board), gs, actionScope ) {
		Board = board;
	}

	public Task SelectActionOption( params IExecuteOn<BoardCtx>[] options ) => SelectActionOption( "Select Power Option", options );
	public Task SelectActionOption( string prompt, params IExecuteOn<BoardCtx>[] options )=> SelectAction_Inner( prompt, options, Present.AutoSelectSingle, this );
	public Task SelectAction_Optional( string prompt, params IExecuteOn<BoardCtx>[] options )=> SelectAction_Inner( prompt, options, Present.Done, this );

	public SpaceToken[] FindTokens( params TokenClass[] tokenClasses ) {
		return Board.Spaces
			.SelectMany(
				s => GameState.Tokens[s]
					.OfAnyClass( tokenClasses )
					.Select( t => new SpaceToken( s, t ) )
			)
			.ToArray();
	}


}
