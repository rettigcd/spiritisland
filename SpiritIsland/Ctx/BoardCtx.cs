namespace SpiritIsland;

/// <summary>
/// Used for events / fear that say "for each board"
/// </summary>
public class BoardCtx : SelfCtx {

	#region private static

	static public Spirit FindSpirit( GameState gameState, int boardIndex)
		=> gameState.Spirits[boardIndex < gameState.Spirits.Length ? boardIndex : 0];

	#endregion

	#region constructors

	public BoardCtx( Spirit spirit, Board board )
		:base(spirit ) {
		Board = board;
	}
	public BoardCtx( GameState gs, Board board )
		: base( FindSpirit(gs,board) ) {
		Board = board;
	}

	#endregion

	public Board Board { get; }

	public IEnumerable<SpaceState> ActiveSpaces => Board.Spaces.Upgrade();

	static public Spirit FindSpirit( GameState gameState, Board board ) {
		int index = 0;
		foreach(Board b in gameState.Island.Boards) {
			if(b == board) 
				return gameState.Spirits[index];
			++index;
		}
		return gameState.Spirits[0];
	}


	public SpaceToken[] FindTokens( params IEntityClass[] tokenClasses ) {
		return Board.Spaces
			.SelectMany( s => GameState.Tokens[s].SpaceTokensOfAnyClass( tokenClasses ) )
			.ToArray();
	}


}
