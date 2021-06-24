using System;

namespace SpiritIslandCmd {
	public interface IPhase {
		void Initialize();
		string Prompt { get; }
		bool Handle(string cmd);
		event Action Complete;
	}

}
