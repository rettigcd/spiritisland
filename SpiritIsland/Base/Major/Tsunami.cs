using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	// major
	[MajorCard(Tsunami.Name,6,Speed.Slow,Element.Water,Element.Earth)]
	public class Tsunami : BaseAction {

		public const string Name = "Tsunami";

		public Tsunami(Spirit spirit,GameState gs):base(spirit,gs){ _ = ActAsync(spirit); }

		async Task ActAsync(Spirit self){

			Space space = await engine.TargetSpace_SacredSite(2,s=>s.IsCostal);

			// 2 fear
			gameState.AddFear(2);
			// +8 damage
			gameState.DamageInvaders(space,8);
			// destroy 2 dahan
			int count = System.Math.Min(gameState.GetDahanOnSpace(space),2);
			gameState.AddDahan(space,-count);

			if(self.Elements(Element.Water) >=3			// !!! switch to tell, don't ask
				&& self.Elements(Element.Earth) >= 2
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
						gameState.AddDahan(otherCoast,-1);
				}
			}

		} 
		
	}

}
