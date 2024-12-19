namespace SpiritIsland.JaggedEarth;

public class Russia : AdversaryBuilder, IAdversaryBuilder {

	// https://spiritislandwiki.com/index.php?title=Russia

	public const string Name = "Russia";

	public Russia() : base(Name) { }

	public override AdversaryLevel[] Levels => _levels;

	public override AdversaryLossCondition LossCondition => Russia_Level1_HuntersBringHomeShelAndHide.HuntersSwarmTheIsland;

	readonly AdversaryLevel[] _levels = [
		// Level 0 - Escalation
		new AdversaryLevel(0, 1, 3,3,3, "Stalk the Predators", "Add 2 explorers/board to lands with beast." )
			.WithEscalation( Escalation_StalkThePredators ),

		// Level 1
		new AdversaryLevel(1, 3, 3,3,4, "Hunters Bring Home Shell and Hide", 
			"During Setup, on each board, add 1 beast and 1 Explorer to the highest-numbered land without Town/City. "+
			"During Play, Explorer do +1 Damage. " +
			"When Ravage adds Blight to a land (including cascades), Destroy 1 beast in that land." ) {

			AdjustFunc = (gameState, _)=>{
				// add 1 beast and 1 explorer to highest number land without Town/City
				var highestSpaces = gameState.Island.Boards
					.Select( board => board.Spaces
						.ScopeTokens()
						.Where( x => x.SumAny( Human.Town_City ) == 0 )
						.Last()
					).ToArray();
				foreach(var highestLandWithoutTownCity in highestSpaces) {
					highestLandWithoutTownCity.Setup(Human.Explorer,1);
					highestLandWithoutTownCity.Setup(Token.Beast,1);
				}
			},

			InitFunc = (gameState,_) => {
				// Explorers do +1 damage
				gameState.Tokens.TokenDefaults[Human.Explorer] = ((HumanToken)gameState.Tokens.TokenDefaults[Human.Explorer]).SetAttack( 2 );
				// When Ravage adds Blight to a land, destroy 1 beast
				gameState.AddIslandMod( new Russia_Level1_HuntersBringHomeShelAndHide() );
			},
		},

		// Level 2
		new AdversaryLevel(2, 4, 4,3,4, "A Sense for Impending Disaster", 
			"The first time each Action would Destroy Explorer, push it instead (+1 fear)" ) {
			InitFunc = (gameState,_) => {
				gameState.AddIslandMod( new Russia_Level2_SenseOfPendingDisasterMod() );
			}
		},

		// Level 3
		new AdversaryLevel(3, 6, 4,4,3, "Competition Among Hunters", "Ravage Cards also match lands with 3 or more Explorer." ){
			InitFunc = (gameState,adv) => {
				if(adv.Level < 6)
					gameState.InvaderDeck.Ravage.Engine = new Russia_Level3_CompetitionAmongHuntersRavageEngine();
			}
		},

		// Level 4
		new AdversaryLevel(4, 7, 4,4,4, "Accelerated Exploitation").WithInvaderCardOrder("111-2-3-2-3-2-3-2-33"),

		// Level 5
		new AdversaryLevel(5, 9, 4,5,4, "Entrench in the Face of Fear",  
			"Add Stage II Invader Card under 3rd Fear Card, and Stage III under 7th Fear Cards." ) {
			InitFunc = (gameState,_) => {
				// Fear Card #3 and #7 add Build Card
				IFearCard[] fearCards = [.. gameState.Fear.Deck];
				gameState.Fear.CardActivated += new AddBuildWhenFearActivated(fearCards[3-1],gameState.InvaderDeck.TakeNextUnused(2)).WatchActivatedCard;
				gameState.Fear.CardActivated += new AddBuildWhenFearActivated(fearCards[7-1],gameState.InvaderDeck.TakeNextUnused(3)).WatchActivatedCard;
			}
		},

		// Level 6
		new AdversaryLevel(6, 11, 5,5,4, "Pressure for Fast Profit", 
			"After Ravage, on each board where it added no Blight: In the land with the most Explorer (min. 1), add 1 Explorer and 1 Town." ) {
			InitFunc = (gameState,_) => gameState.InvaderDeck.Ravage.Engine = new Russia_Level6_PressureForFastProfitRavageEngine(gameState)
		}
	];

	class AddBuildWhenFearActivated(IFearCard fearCard, InvaderCard invaderCard) {
		public void WatchActivatedCard( IFearCard activatedFearCard) {
			if(activatedFearCard == fearCard ) {
				GameState.Current.InvaderDeck.Build.Cards.Add(invaderCard);
				ActionScope.Current.LogDebug($"Entrenched in the Face of Fear: Adding invader card {invaderCard.Code} to Builds.");
			}
		}
	}

	#region Escalation

	static async Task Escalation_StalkThePredators( GameState gameState ) {

		var ambient = GameState.Current;

		// Add 2 explorers per board to lands with beast.
		Dictionary<Board, Space[]> beastsSpacesForBoard = gameState.Island.Boards
			.Select( b => new { board=b, spaces= SpacesWithBeasts(b) } )
			.Where( x=>0<x.spaces.Length)
			.ToDictionary( x=>x.board, x=>x.spaces );

		// If no beasts anywhere, can't add explorers.
		if(beastsSpacesForBoard.Count == 0) return;

		for(int boardIndex = 0; boardIndex < gameState.Island.Boards.Length; ++boardIndex) {
			Board board = gameState.Island.Boards[boardIndex];
			Spirit spirit = board.FindSpirit();

			bool boardHasBeastSpaces = beastsSpacesForBoard.ContainsKey( board );
			IEnumerable<Space> addSpaces = boardHasBeastSpaces
				? beastsSpacesForBoard[board]
				: beastsSpacesForBoard.Values.SelectMany( x => x );
			for(int i = 0; i < 2; ++i) {
				await using ActionScope actionScope = await ActionScope.Start(ActionCategory.Adversary);
				Space addSpace = await spirit.SelectAlways($"Escalation - Add Explorer for board {board.Name} ({i + 1} of 2)", addSpaces);
				await addSpace.AddDefaultAsync( Human.Explorer, 1, AddReason.Explore );
			}
		}
	}

	static Space[] SpacesWithBeasts( Board board ) 
		=> board.Spaces.ScopeTokens().Where( s => s.Beasts.Any ).ToArray();

	#endregion Escalation

}