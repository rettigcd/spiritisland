using System;

namespace SpiritIslandCmd {
	public class FakePhase : IPhase {

		public FakePhase(string phaseName){
			Prompt = phaseName + " Press 'n' to go to next phase";
		}

		public string Prompt {get;}

		public event Action Complete;

		public bool Handle( string cmd,int _ ) {
			if(cmd != "n") return false;
			this.Complete?.Invoke();
			return true;
		}

		public void Initialize() {
		}
	}

}
