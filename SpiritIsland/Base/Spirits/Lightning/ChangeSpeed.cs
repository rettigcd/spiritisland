namespace SpiritIsland.Base {
	public class ChangeSpeed : IActionFactory {
		public ChangeSpeed(IActionFactory original, Speed newSpeed){
			Original = original;
			Speed = newSpeed;
		}
		public Speed Speed { get; }

		public string Name => Original.Name;

		public string Text => Original.Text;

		public IAction Bind( Spirit spirit, GameState gameState ) => Original.Bind(spirit,gameState);

		public IActionFactory Original { get; }

	}

}
