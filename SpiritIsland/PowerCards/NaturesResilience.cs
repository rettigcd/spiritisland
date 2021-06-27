
namespace SpiritIsland.PowerCards {

	[PowerCard(NaturesResilience.Name,1,Speed.Fast,Element.Earth,Element.Plant,Element.Animal)]
	public class NaturesResilience : BaseAction {

		public const string Name = "Nature's Resilience";

		readonly Space space;
		readonly bool canRemoveBlight;

		public NaturesResilience(Spirit spirit,GameState gameState):base(gameState){
			// if 2 water, you may INSTEAD remove 1 blight
			canRemoveBlight = spirit.Elements(Element.Water)>=2;

			// range 1 from SS
			engine.decisions.Push(new TargetSpaceRangeFromSacredSite(spirit,1,SelectSpace));
		}

		void SelectSpace(Space space){

			if(canRemoveBlight)
				engine.decisions.Push( new SelectText( 
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

		const string DefendKey = "Defend 6";
		const string RemoveBlightKey = "Remove Blight";

		void Defend6()=>gameState.Defend(space,6);
		void RemoveBlight()=>gameState.AddBlight(space,-1);

	}
}
