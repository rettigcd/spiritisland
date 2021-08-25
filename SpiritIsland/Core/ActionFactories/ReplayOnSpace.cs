using System.Threading.Tasks;

namespace SpiritIsland {
	class ReplayOnSpace : IActionFactory {
		readonly TargetSpace_PowerCard original;
		readonly Space target;
		public ReplayOnSpace( TargetSpace_PowerCard original, Space target ) {
			this.original = original;
			this.target = target;
		}

		public Speed Speed {
			get{ return original.Speed; }
			set { original.Speed = value; }
		}

		public string Name => original.Name;

		public string Text => original.Text;

		IActionFactory IActionFactory.Original => original;

		public Task ActivateAsync( Spirit spirit, GameState gameState )
			=> original.ActivateAgainstSpecificTarget(spirit,gameState,target);
		
	}

}
