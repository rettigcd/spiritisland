namespace SpiritIsland.JaggedEarth;

public class Russia : IAdversary {

	public const string Name = "Russia";

	public int Level { get; set; }

	public InvaderDeckBuilder InvaderDeckBuilder => new InvaderDeckBuilder( Level switch {
		4 or 5 or 6 => new int[] { 1, 1, 1, 2, 3, 2, 3, 2, 3, 2, 3, 3 },
		_ => null // use default
	} );

	public int[] FearCardsPerLevel => Level switch {
		1 => new int[] { 3, 3, 4 },
		2 => new int[] { 4, 3, 4 },
		3 => new int[] { 4, 4, 3 },
		4 => new int[] { 4, 4, 4 },
		5 => new int[] { 4, 5, 4 },
		6 => new int[] { 5, 5, 4 },
		_ => null
	};

	public ScenarioLevel[] Adjustments => new ScenarioLevel[] {
		// Level 0 - Escalation
		new ScenarioLevel(1 , 3,3,3, "Stalk the Predators", "Add 2 explorers/board to lands with beast." ),
		// Level 1
		new ScenarioLevel(3 , 3,3,4, "Hunters Bring Home Shell and Hide", 
			"During Setup, on each board, add 1 beast and 1 Explorer to the highest-numbered land without Town/City. "+
			"During Play, Explorer do +1 Damage. " +
			"When Ravage adds Blight to a land (including cascades), Destroy 1 beast in that land." ),
		// Level 2
		new ScenarioLevel(4 , 4,3,4, "A Sense for Impending Disaster", "The first time each Action would Destroy Explorer, push it instead (+1 fear)" ),
		// Level 3
		new ScenarioLevel(6 , 4,4,3, "Competition Among Hunters", "Ravage Cards also match lands with 3 or more Explorer." ),
		// Level 4
		new ScenarioLevel(7 , 4,4,4, "Accelerated Exploitation",  "111-2-3-2-3-2-3-2-33" ),
		// Level 5
		new ScenarioLevel(9 , 4,5,4, "Entrench in the Face of Fear",  "Add Stage II Invader Card under 3rd Fear Card, and Stage III under 7th Fear Cards." ),
		// Level 6
		new ScenarioLevel(11, 5,5,4, "Pressure for Fast Profit", "After Ravage, on each board where it added no Blight: In the land with the most Explorer (min. 1), add 1 Explorer and 1 Town." ),
	};

	public void PreInitialization( GameState gameState ) {

		var ravageEngine = new RussiaRavageEngine( _token );
		gameState.InvaderDeck.Ravage.Engine = ravageEngine;

		// Escalation - Stalk the Predators
		gameState.InvaderDeck.Explore.Engine.Escalation = StalkThePredators;

		// Additional Loss Condition
		gameState.AddWinLossCheck( _token.HuntersSwarmTheIsland );

		// Level 1 - depends on tokens already placed

		// Level 2
		if(2 <= Level)
			_token.HasASenseOfPendingDisaster = true;

		// level-3 - in PreInitialization, setting level in Invader card
		if(3 <= Level)
			ravageEngine.ShouldCheckCompetitionAmongHunters = true;

		// level-4 - nothing

		// level-5 - see Pre-Init
		if(5 <= Level)
			EntrenchInTheFaceOfFear( gameState );

		// level-6 - post-ravage event that adds towns/exporers to boards that didn't get blight.
		if(6 <= Level)
			ravageEngine.CheckForPressureForFastProfit = true;

	}

	public void PostInitialization( GameState gameState ) {
		if(1 <= Level)
			HuntersBringHomeShellAndHide( gameState ); // requires tokens to already be in place
	}

	#region private methods

	static async Task StalkThePredators( GameState gameState ) {
		// Add 2 explorers/board to lands with beast.

		var beastsSpacesForBoard = gameState.AllActiveSpaces
			.Where( s => s.Beasts.Any )
			.GroupBy( s => s.Board.Board )
			.ToDictionary( s => s.Key, s => s.ToArray() );

		// If no beasts anywhere, can't add explorers.
		if(!beastsSpacesForBoard.Any()) return;

		for(int boardIndex = 0; boardIndex < gameState.Island.Boards.Length; ++boardIndex) {
			Board board = gameState.Island.Boards[boardIndex];
			Spirit spirit = BoardCtx.FindSpirit(gameState, board);

			var addSpaces = beastsSpacesForBoard.ContainsKey( board )
				? beastsSpacesForBoard[board]
				: beastsSpacesForBoard.Values.SelectMany( x => x );
			for(int i = 0; i < 2; ++i) {
				await using ActionScope actionScope = new ActionScope( ActionCategory.Adversary );
				var criteria = new Select.ASpace( $"Escalation - Add Explorer for board {board.Name} ({i + 1} of 2)", addSpaces.Downgrade(), Present.Always );
				Space addSpace = await spirit.Gateway.Decision( criteria );
				await gameState.Tokens[addSpace].AddDefault( Human.Explorer, 1, AddReason.Explore );
			}
		}
	}

	static void EntrenchInTheFaceOfFear( GameState gameState ) {
		// Level 5
		// Modify Fear Card #3 and #7 to add Build Card
		var hold = new Stack<IFearCard>();
		var deck = gameState.Fear.Deck;
		hold.Push( deck.Pop() ); // 1
		hold.Push( deck.Pop() ); // 2
		hold.Push( new RussiaFearCard( deck.Pop(), gameState.InvaderDeck.TakeNextUnused(2) ) ); // 3
		hold.Push( deck.Pop() ); // 4
		hold.Push( deck.Pop() ); // 5
		hold.Push( deck.Pop() ); // 6
		hold.Push( new RussiaFearCard( deck.Pop(), gameState.InvaderDeck.TakeNextUnused(3) ) ); // 7
		while(0 < hold.Count)
			deck.Push( hold.Pop() );
	}

	void HuntersBringHomeShellAndHide( GameState gameState ) {
		AddBeastAndExplorer( gameState );
		ExplorersDo2Damage( gameState );
		RavageBlightDestroysBeasts( gameState );
	}

	static void AddBeastAndExplorer( GameState gameState ) {
		// add 1 beast and 1 explorer to highest number land without Town/City
		var highestSpaces = gameState.Island.Boards
			.Select( board => board.Spaces
				.Select( gameState.Tokens.GetTokensFor )
				.Where( x => x.SumAny( Human.Town_City ) == 0 )
				.Last() 
			).ToArray();
		foreach(var highestLandWithoutTownCity in highestSpaces) {
			highestLandWithoutTownCity.AdjustDefault(Human.Explorer,1);
			highestLandWithoutTownCity.Adjust(Token.Beast,1);
		}
		gameState.LogDebug($"Hunters Bring Home Shell and Hide - Added Beast & Explorers to: " + highestSpaces.SelectLabels().OrderBy(s=>s).Join(","));
	}

	static void ExplorersDo2Damage( GameState gameState ) {
		// Change explorers damage to 2
		gameState.Tokens.Attack[Human.Explorer] = 2;

	}

	void RavageBlightDestroysBeasts( GameState gameState ) {
		// Add action that destroys beasts from Ravage-Blight
		gameState.AddToAllActiveSpaces( _token );
	}

	readonly RussiaToken _token = new RussiaToken();

	#endregion

}