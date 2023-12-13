namespace SpiritIsland.JaggedEarth;

/// <summary>
/// Logs which spaces receive blight during ravage.
/// </summary>
class RussiaIslandMod : BaseModEntity, IHandleTokenAddedAsync, IModifyRemovingTokenAsync {

	#region construction

	public RussiaIslandMod() {}

	#endregion

	public bool HasASenseOfPendingDisaster { get; set; }

	public void HuntersSwarmTheIsland( GameState gameState ) {
		// Put beast Destroyed by Adversary rules on this panel.If there are ever more beast on this panel than on the island, the Invaders win.
		int remainingBeasts = gameState.Spaces_Unfiltered.Sum( s => s.Beasts.Count );
		if(remainingBeasts < _beastsDestroyed)
			GameOverException.Lost( $"Russia-Hunters Swarm the Island (beasts remaining:{remainingBeasts} killed:{_beastsDestroyed})" );
	}

	public void PreRavage() {
		_receivedRavageBlight.Clear();
	}
	/// <summary>
	/// Tracks which boards received blight during Ravage - this turn only
	/// </summary>
	readonly HashSet<Board> _receivedRavageBlight = new HashSet<Board>();

	public void PressureForFastProfit( GameState gameState ) {
		// Level 6
		// After the Ravage Step of turn 2+,
		// on each board where it added no Blight:
		var boardsWithNoNewBlight = gameState.Island.Boards.Except( _receivedRavageBlight );

		// In the land with the most Explorer
		static SpaceState PickSpaceWithMostExplorers(Board board) => board.Spaces.Tokens()
			.Where( ss => 0 < ss.Sum( Human.Explorer ) ) //  (min. of 1)
			.OrderByDescending( ss => ss.Sum( Human.Explorer ) )
			.FirstOrDefault();
		var landsWithMostExplorers = boardsWithNoNewBlight
			.Select( PickSpaceWithMostExplorers )
			.Where( l => l != null )
			.ToArray();

		foreach(var land in landsWithMostExplorers) {
			// add 1 Explorer and 1 Town.
			land.AdjustDefault( Human.Explorer, 1 );
			land.AdjustDefault( Human.Town, 1 );
		}
		if( landsWithMostExplorers.Any() )
			gameState.LogDebug("Pressure for Fast Profit: Added 1T+1E to "
				+landsWithMostExplorers.SelectLabels().Order().Join(",")
			);
		
	}

	#region mods

	async Task IHandleTokenAddedAsync.HandleTokenAddedAsync( ITokenAddedArgs args ) {
		if(args.Added == Token.Blight
			&& args.Reason == AddReason.Ravage
		) {
			_receivedRavageBlight.UnionWith( args.To.Boards ); // log

			var beasts = args.To.Tokens.Beasts;
			if( beasts.Any ) {
				await beasts.Destroy( 1 );
				_beastsDestroyed++;
				GameState.Current.LogDebug($"Blight on {args.To.Text} destroys 1 beast.");
			}
		}
	}

	async Task IModifyRemovingTokenAsync.ModifyRemovingAsync( RemovingTokenArgs args ) {
		const string key = "A Sense of Pending Disaster";
		SpaceState[] pushOptions;
		var scope = ActionScope.Current;
		if(HasASenseOfPendingDisaster
			&& args.Token.Class == Human.Explorer     // Is explorer
			&& args.Reason == RemoveReason.Destroyed // destroying
			&& !ActionScope.Current.ContainsKey( key )  // first time
			&& 0 < (pushOptions = args.From.Adjacent_ForInvaders.IsInPlay().ToArray()).Length
		) {
			--args.Count; // destroy one fewer
			scope[key] = true; // don't save any more

			Spirit spirit = scope.Owner ?? args.From.Space.Boards[0].FindSpirit();
			Space destination = await spirit.SelectAsync( A.Space.ToPushToken( (IToken)args.Token, args.From.Space, pushOptions.Downgrade(), Present.Always ) );
			await args.Token.On(args.From.Space).MoveTo( destination );
		}
	}

	#endregion mods

	#region private fields

	int _beastsDestroyed = 0;

	#endregion

}

