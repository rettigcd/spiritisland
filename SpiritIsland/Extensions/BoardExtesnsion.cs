namespace SpiritIsland;

public static class BoardExtesnsion {

	static public Spirit FindSpirit( this Board board ) {
		var gameState = GameState.Current;
		int index = 0;
		foreach(Board b in gameState.Island.Boards) {
			if(b == board)
				return gameState.Spirits[index];
			++index;
		}
		return gameState.Spirits[0];
	}

	static public SpaceToken[] FindTokens( this Board board, params ITokenClass[] tokenClasses ) {
		return board.Spaces
			.SelectMany( s => s.ScopeTokens.SpaceTokensOfAnyTag( tokenClasses ) )
			.ToArray();
	}

}