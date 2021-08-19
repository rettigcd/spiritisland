﻿using SpiritIsland;
using System;

namespace SpiritIsland.SinglePlayer {

	class TimePasses : IPhase {

		public string Prompt => "nothing to do while time passes.";

		readonly GameState gameState;

		public TimePasses(GameState gameState){
			this.gameState = gameState;
		}

		public bool AllowAutoSelect { get; set; } = true;

		public IOption[] Options => Array.Empty<IOption>();

		public event Action Complete;

		public void Initialize() {
			_ = this.gameState.TimePasses();
			this.Complete?.Invoke();
		}

		public void Select( IOption option ) {
			throw new NotImplementedException();
		}
	}

}
