namespace SpiritIsland.Tests;

public static class Boards {
	public static Board A => Board.BuildBoardA();
	public static Board B => Board.BuildBoardB();
	public static Board C => Board.BuildBoardC();
	public static Board D => Board.BuildBoardD();

	public static readonly BoardOrientation Attach1 = GameBuilder.TwoBoardLayout[1];

}
