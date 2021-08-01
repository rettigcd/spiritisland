namespace SpiritIsland.Core {

    public class ChangeSpeed : IActionFactory {

		public ChangeSpeed(IActionFactory original, Speed newSpeed){
			Original = original;
			Speed = newSpeed;
		}
		public Speed Speed { get; }

		public string Name => Original.Name;

		public string Text => Original.Text;

		public void Activate( ActionEngine engine ) {
			Original.Activate(engine);
		}

		public IActionFactory Original { get; }

	}

}
