using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class Tsunami {

		public const string Name = "Tsunami";


		[MajorCard(Tsunami.Name,6,Speed.Slow,Element.Water,Element.Earth)]
		[FromSacredSite(2,Target.Costal)]
		static public async Task ActAsync(ActionEngine engine,Space space){
			var(self,gameState) = engine;
			// 2 fear
			engine.AddFear(2);
			// +8 damage
			await engine.DamageInvaders(space,8);
			// destroy 2 dahan
			int count = System.Math.Min(gameState.GetDahanOnSpace(space),2);
			await engine.GameState.DestoryDahan(space,count,DahanDestructionSource.PowerCard);

			if(self.Elements.Contains("3 water,2 earth")){
				var others = gameState.Island
					.Boards.Single(b=>b[1].Label[0]==space.Label[0])
					.Spaces.Where(s=>s.IsCostal && s != space)
					.ToArray();
				foreach(var otherCoast in others){
					engine.AddFear(1);
					// 4 damage
					await engine.DamageInvaders(otherCoast,4);
					// destroy 1 dahan
					if(gameState.HasDahan(otherCoast))
						await gameState.DestoryDahan(otherCoast,1, DahanDestructionSource.PowerCard);
				}
			}
		}

	}

}
