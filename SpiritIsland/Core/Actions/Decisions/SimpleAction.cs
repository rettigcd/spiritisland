namespace SpiritIsland.Core {
	public class SimpleAction : IAtomicAction {
		readonly System.Action action;

		public SimpleAction(System.Action action ){
			this.action = action;
		}

		public void Apply( GameState _ ) {
			action();
		}

	}

}
