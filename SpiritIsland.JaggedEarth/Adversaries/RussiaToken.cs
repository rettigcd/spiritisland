using static System.Formats.Asn1.AsnWriter;

namespace SpiritIsland.JaggedEarth;

class RussiaToken : ISpaceEntity, IHandleTokenAdded, IHandleRemovingToken {

	#region construction

	public RussiaToken() {}

	#endregion

	public IEntityClass Class => ActionModTokenClass.Singleton;

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

	async Task IHandleTokenAdded.HandleTokenAdded( ITokenAddedArgs args ) {
		if(args.Token == Token.Blight
			&& args.Reason == AddReason.Ravage
		) {
			_receivedRavageBlight.Add( args.AddedTo.Space.Board );// log

			if( args.AddedTo.Beasts.Any ) {
				await args.AddedTo.Beasts.Destroy( 1 );
				_beastsDestroyed++;
				GameState.Current.LogDebug($"Blight on {args.AddedTo.Space.Text} destroys 1 beast.");
			}
		}
	}

	async Task IHandleRemovingToken.ModifyRemoving( RemovingTokenArgs args ) {
		const string key = "A Sense of Pending Disaster";
		SpaceState[] pushOptions;
		var scope = UnitOfWork.Current;
		if(HasASenseOfPendingDisaster
			&& args.Token.Class == Human.Explorer     // Is explorer
			&& args.Reason == RemoveReason.Destroyed // destroying
			&& !UnitOfWork.Current.ContainsKey( key )  // first time
			&& 0 < (pushOptions = args.Space.Adjacent_ForInvaders.Where( ss => scope.TerrainMapper.IsInPlay( ss ) ).ToArray()).Length
		) {
			--args.Count; // destroy one fewer
			scope[key] = true; // don't save any more

			GameState gs = GameState.Current;
			Spirit spirit = scope.Owner ?? BoardCtx.FindSpirit( gs, args.Space.Space.Board );
			Space destination = await spirit.Gateway.Decision( Select.Space.PushToken( (IToken)args.Token, args.Space.Space, pushOptions, Present.Always ) );
			await args.Space.MoveTo( (IToken)args.Token, destination );
		}
	}

	#endregion mods

	#region private fields

	int _beastsDestroyed = 0;

	#endregion

}

