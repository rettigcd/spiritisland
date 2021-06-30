using System;
using SpiritIsland;

namespace SpiritIslandCmd {
	public interface IPhase {
		void Initialize();
		event Action Complete;
		public UiMap uiMap { get; }
		public void Select( IOption option );
	}

}
