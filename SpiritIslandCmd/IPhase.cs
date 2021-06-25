using System;

namespace SpiritIslandCmd {
	public interface IPhase {
		void Initialize();
		string Prompt { get; }
		bool Handle(string cmd,int index);
		event Action Complete;
	}

}
