using SpiritIsland;
using System;

namespace SpiritIslandCmd {
	public class TimePasses : IPhase {
		
		public string Prompt => uiMap.ToPrompt();

		readonly Spirit spirit;

		public TimePasses(Spirit spirit,GameState _){
			this.spirit = spirit;
			this.uiMap = new UiMap("nothing to do while time passes.",Array.Empty<IOption>(),null);
		}

		public UiMap uiMap { get; set; }

		public event Action Complete;

		public void Initialize() {
			// should this be in GameState.TimePasses???
			this.spirit.DiscardPile.AddRange(this.spirit.PurchasedCards);
			this.spirit.PurchasedCards.Clear();
			// !!! heal dahan & invaders
			this.Complete?.Invoke();
		}

		public void Select( IOption option ) {
			throw new NotImplementedException();
		}
	}

}
