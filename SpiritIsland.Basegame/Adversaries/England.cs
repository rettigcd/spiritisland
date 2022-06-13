namespace SpiritIsland.Basegame;

public class England : IAdversary {

	public int Level { get; set; }

	public int[] InvaderCardOrder => null;

	public int[] FearCardsPerLevel => Level switch {
		1 => new int[] { 3, 4, 3 },
		2 => new int[] { 4, 4, 3 },
		3 => new int[] { 4, 5, 4 },
		4 => new int[] { 4, 5, 5 },
		5 => new int[] { 4, 5, 5 },
		6 => new int[] { 4, 5, 4 },
		_ => null,
	};

	public void Adjust( GameState gameState ) {
		if( Level >= 2) {
			// During Setup, on each board add 1 City to land #1 and 1 Town to land #2.
			foreach(var board in gameState.Island.Boards) {
				gameState.Tokens[board[1]].AdjustDefault( Invader.City, 1 );
				gameState.Tokens[board[2]].AdjustDefault( Invader.Town, 1 );
			}
		}
	}

	public void AdjustInvaderDeck( InvaderDeck deck ) {
		if(0 < Level)
			Level1_AdditionalBuildSpaces( deck );
	}

	static void Level1_AdditionalBuildSpaces( InvaderDeck deck ) {
		for(int i = 0; i < deck.UnrevealedCards.Count; ++i) {
			if(deck.UnrevealedCards[i] is InvaderCard normalInvaderCard)
				deck.UnrevealedCards[i] = new EnglandInvaderCard( normalInvaderCard );
		}
	}

	public class EnglandInvaderCard : InvaderCard {
		readonly InvaderCard card;
		public EnglandInvaderCard(InvaderCard card):base(card.Text,card.InvaderStage,card.Escalation) {
			this.card = card;
		}
		public override bool Matches( Space space ) => card.Matches( space );

		protected override bool ShouldBuildOnSpace( TokenCountDictionary tokens, GameState gameState ) {
			int cityTownCounts(Space space) => gameState.Tokens[space].SumAny( Invader.Town, Invader.City );
			bool adjacentTo2OrMoreCitiesOrTowns(Space space) => 2 <= space.Adjacent.Sum( adj => cityTownCounts( adj ) );
			return base.ShouldBuildOnSpace( tokens, gameState ) || adjacentTo2OrMoreCitiesOrTowns(tokens.Space);
		}
	}

}


/*

1	(3)	10 (3/4/3)	Indentured Servants Earn Land: 
	Invader Build Cards affect matching lands without Invaders if they are adjacent to at least 2 Towns/Citys.

2	(4)	11 (4/4/3)	Criminals and Malcontents: 
	During Setup, on each board add 1 City to land #1 and 1 Town to land #2.

3	(6)	13 (4/5/4)	High Immigration (I): 
	Put the "High Immigration" tile on the Invader board, to the left of "Ravage".
	The Invaders take this Build action each Invader phase before Ravaging.
	Cards slide left from Ravage to it, and from it to the discard pile. 
	Remove the tile when a Stage II card slides onto it, putting that card in the discard.
	// Create Invader Card Action Piles.

4	(7)	14 (4/5/5)	High Immigration (full): 
	The extra Build tile remains out the entire game.

5	(9)	14 (4/5/5)	Local Autonomy: 
	Townicon.png/Cityicon.png have +1 Health.

6	(11)	13 (4/5/4)	Independent Resolve: 
	During Setup, add an additional Fearicon.png to the Fear Pool per player in the game. 
	During any Invader Phase where you resolve no Fear Cards, perform the Build from High Immigration twice. 
	(This has no effect if no card is on the extra Build slot.)

*/