﻿namespace SpiritIsland.Core {
    class ReplayOnSpace : IActionFactory {
		readonly TargetSpace_PowerCard original;
		readonly Space target;
		public ReplayOnSpace( TargetSpace_PowerCard original, Space target ) {
			this.original = original;
			this.target = target;
		}

		public Speed Speed => original.Speed;

		public string Name => original.Name;

		public string Text => original.Text;

        IActionFactory IActionFactory.Original => original;

        public void Activate( ActionEngine engine )
			=> original.Activate(engine,target);
		
	}

}
