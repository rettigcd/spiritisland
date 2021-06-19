namespace SpiritIsland.PowerCards {
	public class BaseAction {
		protected BaseAction(GameState gameState){
			this.gameState = gameState;
		}
		protected void AutoSelectSingleOptions() {
			var opt = Options;
			while (opt.Length == 1) {
				InnerSelect(opt[0]);
				opt = Options;
			}
		}

		public bool IsResolved => Options.Length == 0;

		public void Apply() {
			foreach(var move in engine.moves)
				move.Apply( gameState );
		}

		public void Select(IOption option) {
			InnerSelect(option);
			AutoSelectSingleOptions();
		}

		public IOption[] Options {
			get{
				if(engine.decisions.Count>0)
					return engine.decisions.Peek().Options;
	
				return new IOption[0];
			}
		}

		protected void InnerSelect(IOption option) {
			if(engine.decisions.Count>0){
				var descision = engine.decisions.Pop();
				descision.Select(option,engine);
				return;
			}

			throw new System.NotImplementedException();
		}

		protected readonly GameState gameState;
		protected readonly ActionEngine engine = new ActionEngine();
	}

}
