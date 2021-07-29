using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	// Lightnong - Special Rule

	public class SwiftnessOfLightning : IActionFactory {
		public Speed Speed => Speed.Fast;

		public string Name => "Swiftness of Lightning";

		public IActionFactory Original => this;

		public string Text => Name;

		// Lightning Spirit calls this when it activates its cards
		public void OnActivateCards(Spirit lightning){
			if(lightning.Elements[Element.Air]>0)
				lightning.AddActionFactory( this );
		}

		public IAction Bind( Spirit spirit, GameState gameState ) {
			int airCount = spirit.Elements[Element.Air];
			return new SwiftAction(spirit,gameState,airCount);
		}

		class SwiftAction : BaseAction { // This could be refactored to just take the starting decision.
			public SwiftAction(Spirit lightning,GameState gs,int count):base(lightning,gs){
				_ = engine.SelectActionsAndMakeFast( lightning, count );
			}
		}
	}

}
