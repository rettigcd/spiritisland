namespace SpiritIsland.Tests;

public static class Boards {
	public static Board A => BoardFactory.BuildA();
	public static Board B => BoardFactory.BuildB();
	public static Board C => BoardFactory.BuildC();
	public static Board D => BoardFactory.BuildD();
	public static Board E => BoardFactory.BuildE();

	public static readonly BoardOrientation Attach1 = GameBuilder.TwoBoardLayout[1];

}
