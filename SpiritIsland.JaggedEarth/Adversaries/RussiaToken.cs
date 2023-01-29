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
			GameOverException.Lost( $"Russia-Hunters Swarm the Island (beasts remaining:{remainingBeasts} killed:{_beastsDestroyed})" );
	}

	public void PreRavage() {
		_receivedRavageBlight.Clear();
	}
	readonly HashSet<Board> _receivedRavageBlight = new HashSet<Board>();

	public void PressureForFastProfit( GameState gameState ) {
		// Level 6
		// After the Ravage Step of turn 2+,
		// on each board where it added no Blight:
		var boardsWithNoNewBlight = gameState.Island.Boards.Except( _receivedRavageBlight );

		// In the land with the most Explorer
		SpaceState PickSpaceWithMostExplorers(Board board) => gameState.Tokens.PowerUp( board.Spaces )
			.Where( ss => 0 < ss.Sum( Invader.Explorer ) ) //  (min. of 1)
			.OrderByDescending( ss => ss.Sum( Invader.Explorer ) )
			.FirstOrDefault();
		var landsWithMostExplorers = boardsWithNoNewBlight
			.Select( PickSpaceWithMostExplorers )
			.Where( l => l != null )
			.ToArray();

		foreach(var land in landsWithMostExplorers) {
			// add 1 Explorer and 1 Town.
			land.AdjustDefault( Invader.Explorer, 1 );
			land.AdjustDefault( Invader.Town, 1 );
		}
		if( landsWithMostExplorers.Any() )
			gameState.LogDebug("Pressure for Fast Profit: Added 1T+1E to "
				+landsWithMostExplorers.Select(x=>x.Space.Text).Order().Join(",")
			);
		
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
				args.GameState.LogDebug($"Blight on {args.AddedTo.Space.Text} destroys 1 beast.");
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
			Space destination = await spirit.Gateway.Decision( Select.Space.PushToken( (IVisibleToken)args.Token, args.Space.Space, pushOptions, Present.Always ) );
			await args.Space.MoveTo( args.Token, destination );
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

