namespace SpiritIsland {

	public interface IActionFactory : IOption {
		IAction Bind(Spirit spirit,GameState gameState);
		Speed Speed { get; }
		string Name { get; }
		IActionFactory Original { get; }
	}

}