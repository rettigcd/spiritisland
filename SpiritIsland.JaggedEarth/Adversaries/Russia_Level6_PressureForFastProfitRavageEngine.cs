namespace SpiritIsland.JaggedEarth;

/// <summary>
/// Adds ravage spaces where 3 or more explorers
/// Level-6 => CheckForPressureForFastProfit - Adds Explorer/Town to each board where no blight was added
/// </summary>
class Russia_Level6_PressureForFastProfitRavageEngine : Russia_Level3_CompetitionAmongHuntersRavageEngine {

	public Russia_Level6_PressureForFastProfitRavageEngine( GameState gameState ) {
		_token = new RecordBlightAdded();
		gameState.AddIslandMod( _token );
	}

	// After Ravage, on each board where it added no Blight: In the land with the most Explorer( min. 1), add 1 Explorer and 1 Town.
	public override async Task ActivateCard( InvaderCard card ) {
		_token.PreRavage();
		await base.ActivateCard( card );
		await _token.PressureForFastProfit();
	}

	readonly RecordBlightAdded _token;

	class RecordBlightAdded : BaseModEntity, IHandleTokenAdded {

		public void PreRavage() {
			_receivedRavageBlight.Clear();
		}

		/// <summary>
		/// Tracks which boards received blight during Ravage - this turn only
		/// </summary>
		readonly HashSet<Board> _receivedRavageBlight = [];

		Task IHandleTokenAdded.HandleTokenAddedAsync( Space to, ITokenAddedArgs args ) {
			if(args.Added == Token.Blight
				&& args.Reason == AddReason.Ravage
			) {
				_receivedRavageBlight.UnionWith( to.SpaceSpec.Boards ); // log
			}
			return Task.CompletedTask;
		}

		public async Task PressureForFastProfit() {
			// Level 6
			// After the Ravage Step of turn 2+,
			// on each board where it added no Blight:
			var boardsWithNoNewBlight = GameState.Current.Island.Boards.Except( _receivedRavageBlight );

			// In the land with the most Explorer
			static Space PickSpaceWithMostExplorers( Board board ) => board.Spaces.ScopeTokens()
				.Where( ss => 0 < ss.Sum( Human.Explorer ) ) //  (min. of 1)
				.OrderByDescending( ss => ss.Sum( Human.Explorer ) )
				.FirstOrDefault();
			var landsWithMostExplorers = boardsWithNoNewBlight
				.Select( PickSpaceWithMostExplorers )
				.Where( l => l != null )
				.ToArray();

			foreach(var land in landsWithMostExplorers) {
				// add 1 Explorer and 1 Town.
				await land.AddDefaultAsync( Human.Explorer, 1 );
				await land.AddDefaultAsync( Human.Town, 1 );
			}
			if(landsWithMostExplorers.Length != 0)
				ActionScope.Current.LogDebug( "Pressure for Fast Profit: Added 1T+1E to "
					+ landsWithMostExplorers.SelectLabels().Order().Join( "," )
				);
		}

	}

}
