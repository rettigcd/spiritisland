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

	public void Adjust( GameState board ) {
		// Level 1 ->
	}
}


/*

1	(3)	10 (3/4/3)	Indentured Servants Earn Land: 
	Invader Build Cards affect matching lands without Invaders if they are adjacent to at least 2 Towns/Citys.
	// Modify the Build Engine

2	(4)	11 (4/4/3)	Criminals and Malcontents: 
	During Setup, on each board add 1 Cityicon.png to land #1 and 1 Townicon.png to land #2.
	// Adjust GameState - hook ready...

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