namespace SpiritIsland.JaggedEarth;

class RussiaToken : BaseModToken, IHandleTokenAdded, IHandleRemovingToken {

	#region construction

	public RussiaToken() : base( "Russia", UsageCost.Free, true ) { }

	#endregion

	public bool HasASenseOfPendingDisaster { get; set; }

	public void HuntersSwarmTheIsland( GameState gameState ) {
		// Put beast Destroyed by Adversary rules on this panel.If there are ever more beast on this panel than on the island, the Invaders win.
		int remainingBeasts = gameState.AllSpaces.Sum( s => s.Beasts.Count );
		if(remainingBeasts < _beastsDestroyed)
			GameOverException.Lost( $"Russia-Hunters Swarm the Island (remaining:{remainingBeasts} killed:{_beastsDestroyed})" );
	}

	public void PreRavage() {
		_receivedRavageBlight.Clear();
	}
	readonly HashSet<Board> _receivedRavageBlight = new HashSet<Board>();

	public void PressureForFastProfit( GameState gameState ) {
		// After the Ravage Step of turn 2+,
		// on each board where it added no Blight:
		foreach(Board board in gameState.Island.Boards.Except( _receivedRavageBlight ) ) {

			// In the land with the most Explorer
			var landWithMostExplorers = gameState.Tokens.PowerUp( board.Spaces )
				//  (min. 1),
				.Where( ss => 0 < ss.Sum( Invader.Explorer ) )
				.OrderByDescending( ss => ss.Sum(Invader.Explorer) )
				.FirstOrDefault();

			// add 1 Explorer and 1 Town.
			landWithMostExplorers?.AdjustDefault( Invader.Explorer, 1 );
			landWithMostExplorers?.AdjustDefault( Invader.Town, 1 );
		}
	}

	#region mods

	async Task IHandleTokenAdded.HandleTokenAdded( ITokenAddedArgs args ) {
		if(args.Token == TokenType.Blight
			&& args.Reason == AddReason.Ravage
		) {
			_receivedRavageBlight.Add( args.AddedTo.Space.Board );// log

			if( args.AddedTo.Beasts.Any ) {
				await args.AddedTo.Beasts.Bind( args.ActionScope ).Destroy( 1 );
				_beastsDestroyed++;
			}
		}
	}

	async Task IHandleRemovingToken.ModifyRemoving( RemovingTokenArgs args ) {
		const string key = "A Sense of Pending Disaster";
		SpaceState[] pushOptions;
		if(HasASenseOfPendingDisaster
			&& args.Token.Class == Invader.Explorer     // Is explorer
			&& args.Reason == RemoveReason.Destroyed   // destroying
			&& args.Space[TokenType.Isolate] == 0 // not isolated
			&& !args.ActionScope.ContainsKey( key ) // first time
			&& 0 < (pushOptions = args.Space.Adjacent.Where( ss => args.ActionScope.TerrainMapper.IsInPlay( ss ) && ss[TokenType.Isolate] == 0 ).ToArray()).Length
		) {
			--args.Count; // destroy one fewer
			args.ActionScope[key] = true; // don't save any more

			GameState gs = args.Space.AccessGameState();
			Spirit spirit = args.ActionScope.Owner ?? FindSpiritForBoard( gs, args.Space.Space.Board );
			Space destination = await spirit.Gateway.Decision( Select.Space.PushToken( args.Token, args.Space.Space, pushOptions, Present.Always ) );
			await args.Space.MoveTo( args.Token, destination, args.ActionScope );
		}
	}

	static Spirit FindSpiritForBoard( GameState gameState, Board board ) {
		// !!! bug if board is added / removed
		for(int i = 0; i < gameState.Spirits.Length; ++i)
			if(gameState.Island.Boards[i] == board)
				return gameState.Spirits[i];
		throw new ArgumentOutOfRangeException( nameof( board ) );
	}

	#endregion mods

	#region private fields

	int _beastsDestroyed = 0;

	#endregion

}

