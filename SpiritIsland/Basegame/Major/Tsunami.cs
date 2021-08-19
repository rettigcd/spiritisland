using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class Tsunami {

		public const string Name = "Tsunami";


		[MajorCard(Tsunami.Name,6,Speed.Slow,Element.Water,Element.Earth)]
		[FromSacredSite(2,Target.Costal)]
		static public async Task ActAsync(TargetSpaceCtx ctx){
			var(self,gameState) = ctx;
			// 2 fear
			ctx.AddFear(2);
			// +8 damage
			await ctx.DamageInvaders( ctx.Target, 8);
			// destroy 2 dahan
			int count = System.Math.Min(gameState.GetDahanOnSpace( ctx.Target ),2);
			await ctx.GameState.DestoryDahan( ctx.Target, count,DahanDestructionSource.PowerCard);

			if(self.Elements.Contains("3 water,2 earth")){
				var others = gameState.Island
					.Boards.Single(b=>b[1].Label[0]== ctx.Target.Label[0])
					.Spaces.Where(s=>s.IsCostal && s != ctx.Target )
					.ToArray();
				foreach(var otherCoast in others){
					ctx.AddFear(1);
					// 4 damage
					await ctx.DamageInvaders(otherCoast,4);
					// destroy 1 dahan
					if(gameState.HasDahan(otherCoast))
						await gameState.DestoryDahan(otherCoast,1, DahanDestructionSource.PowerCard);
				}
			}
		}

	}

}
