using System.Linq;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	// major
	[PowerCard(Tsunami.Name,6,Speed.Slow,Element.Water,Element.Earth)]
	public class Tsunami : TargetSpaceAction {

		public const string Name = "Tsunami";

		readonly bool damageOtherCoasts;

		public Tsunami(Spirit spirit,GameState gs)
			:base(spirit,gs,2,From.SacredSite)
		{
			damageOtherCoasts = spirit.Elements(Element.Water) >=3
				&& spirit.Elements(Element.Earth) >= 2;
		}

		protected override bool FilterSpace( Space space ) => space.IsCostal;

		protected override void SelectSpace(Space space){
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
