using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Basegame {

	public class Tsunami {

		public const string Name = "Tsunami";


		[MajorCard(Tsunami.Name,6,Speed.Slow,Element.Water,Element.Earth)]
		[FromSacredSite(2,Filter.Costal)]
		static public Task ActAsync(ActionEngine engine,Space space){
			var(self,gameState) = engine;
			// 2 fear
			gameState.AddFear(2);
			// +8 damage
			gameState.DamageInvaders(space,8);
			// destroy 2 dahan
			int count = System.Math.Min(gameState.GetDahanOnSpace(space),2);
			gameState.AdjustDahan(space,-count);

			if(self.Elements[Element.Water] >=3			// !!! switch to tell, don't ask
				&& self.Elements[Element.Earth] >= 2
			){
				var others = gameState.Island
					.Boards.Single(b=>b[1].Label[0]==space.Label[0])
					.Spaces.Where(s=>s.IsCostal && s != space)
					.ToArray();
				foreach(var otherCoast in others){
					gameState.AddFear(1);
					// 4 damage
					gameState.DamageInvaders(otherCoast,4);
					// destroy 1 dahan
					if(gameState.HasDahan(otherCoast))
						gameState.AdjustDahan(otherCoast,-1);
				}
			}
			return Task.CompletedTask;
		}

	}

}
