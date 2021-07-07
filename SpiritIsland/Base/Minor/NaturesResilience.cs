using SpiritIsland.Core;

namespace SpiritIsland.Base {

	[MinorCard(NaturesResilience.Name,1,Speed.Fast,Element.Earth,Element.Plant,Element.Animal)]
	public class NaturesResilience : TargetSpaceAction {

		public const string Name = "Nature's Resilience";
		const string DefendKey = "Defend 6";
		const string RemoveBlightKey = "Remove Blight";

		readonly bool canRemoveBlight;
		Space space;

		public NaturesResilience(Spirit self,GameState gameState):base(self,gameState,1,From.SacredSite){
			// if 2 water, you may INSTEAD remove 1 blight
			canRemoveBlight = self.Elements(Element.Water)>=2;
		}

		protected override void SelectSpace(Space space){
			this.space = space;

			if(canRemoveBlight)
				engine.decisions.Push( new SelectText( engine, 
					new string[] { DefendKey, RemoveBlightKey }, 
					SelectEffect
				));
			else
				Defend6();

		}

		void SelectEffect(string option,ActionEngine engine){
			switch(option){
				case DefendKey:	      Defend6();       break;
				case RemoveBlightKey: RemoveBlight(); break;
			}
		}

		void Defend6()=>gameState.Defend(space,6);
		void RemoveBlight()=>gameState.RemoveBlight(space);

	}
}
