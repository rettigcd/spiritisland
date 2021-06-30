﻿namespace SpiritIsland {

	public interface IActionFactory : IOption {
		IAction Bind(Spirit spirit,GameState gameState);
		Speed Speed { get; }
		string Name { get; }
		void Resolved(Spirit spirit);
	}

}