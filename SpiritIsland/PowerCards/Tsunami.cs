using System.Linq;

namespace SpiritIsland.PowerCards {

	// major
	[PowerCard("Tsunami",6,Speed.Slow,Element.Water,Element.Earth)]
	public class Tsunami : BaseAction {

		readonly bool damageOtherCoasts;

		public Tsunami(Spirit spirit,GameState gs):base(gs){

			damageOtherCoasts = spirit.Elements(Element.Water) >=3
				&& spirit.Elements(Element.Earth) >= 2;

			engine.decisions.Push(new TargetSpaceRangeFromSacredSite(spirit,2,s=>s.IsCostal,SelectCoast));
		}

		void SelectCoast(IOption option){
			Space space = option as Space;
			gameState.AddFear(2);
			// add damage of 8
			// destroy 2 dahan

			if(damageOtherCoasts){
				var other = gameState.Island
					.Boards.Single(b=>b[1].Label[0]==space.Label[0])
					.Spaces.Where(s=>s.IsCostal && s != space)
					.ToArray();
				foreach(var o in other){
					gameState.AddFear(1);
					// 4 damage
					// destroy 1 dahan
				}
			}
		}
		
	}

}
