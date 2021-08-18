using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

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

		public Task Activate( ActionEngine engine ) {
			return engine.Self.SelectActionsAndMakeFast( engine.Self.Elements[Element.Air] );
		}

	}

}
