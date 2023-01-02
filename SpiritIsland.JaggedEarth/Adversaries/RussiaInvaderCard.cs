namespace SpiritIsland.JaggedEarth;

// Adds Escalation
class RussiaInvaderCard : InvaderCard {

	readonly bool _hasCompetitionAmongHunters;
	readonly bool _hasPressureForFastProfit;

	readonly RussiaToken _token;
	public RussiaInvaderCard( InvaderCard card, int level, RussiaToken token ) : base( card ) {
		Escalation = StalkThePredators;
		_hasCompetitionAmongHunters = 3 <= level;
		_hasPressureForFastProfit = 6 <= level;
		_token = token;
	}

	protected override bool MatchesCardForRavage( SpaceState spaceState ) => MatchesCard( spaceState )
		|| _hasCompetitionAmongHunters && 3 <= spaceState.Sum( Invader.Explorer );

	static async Task StalkThePredators( GameState gameState ) {
		// Add 2 explorers/board to lands with beast.

		var beastsSpacesForBoard = gameState.AllActiveSpaces
			.Where(s=>s.Beasts.Any)
			.GroupBy(s=>s.Board.Board)
			.ToDictionary(s=>s.Key,s=>s.ToArray());

		using var uow = gameState.StartAction( ActionCategory.Adversary ); // !!! ??? should this be 1 for everything or 1/board or 1/space

		// If no beasts anywhere, can't add explorers.
		if(!beastsSpacesForBoard.Any()) return;

		for(int boardIndex=0; boardIndex < gameState.Island.Boards.Length; ++boardIndex) {
			Board board = gameState.Island.Boards[boardIndex];
			Spirit spirit = gameState.Spirits[boardIndex]; // !!! wrong if board is added or removed.

			var addSpaces = beastsSpacesForBoard.ContainsKey(board)
				? beastsSpacesForBoard[board]
				: beastsSpacesForBoard.Values.SelectMany(x=>x);
			for(int i = 0; i < 2; ++i) {
				var criteria = new Select.Space( $"Escalation - Add Explorer for board {board.Name} ({i+1} of 2)", addSpaces.Select( x => x.Space ), Present.Always );
				var addSpace = await spirit.Gateway.Decision( criteria );
				await gameState.Tokens[addSpace].AddDefault( Invader.Explorer, 1, uow, AddReason.Explore);
			}
		}
	}

	// After Ravage, on each board where it added no Blight: In the land with the most Explorer( min. 1), add 1 Explorer and 1 Town.
	public override async Task Ravage( GameState gameState ) {
		_token.PreRavage();
		await base.Ravage( gameState );
		if(_hasPressureForFastProfit)
			_token.PressureForFastProfit( gameState );
	}

}

