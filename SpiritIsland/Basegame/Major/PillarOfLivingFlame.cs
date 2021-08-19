using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class PillarOfLivingFlame {

		[MajorCard("Pillar of Living Flame",5,Speed.Slow,Element.Fire)]
		[FromSacredSite(2)]
		static public async Task ActionAsync(TargetSpaceCtx ctx){
			var (self,gameState) = ctx;

			// 3 fear, 5 damage
			// if you have 4 fire
			bool hasBonus = self.Elements[Element.Fire]>=4;
			// +2 fear, +5 damage
			ctx.AddFear( 3 + (hasBonus ? 2 : 0) );
			await ctx.DamageInvaders( ctx.Target, 5 + (hasBonus ? 5 : 0));

			// if target is Jungle / Wetland, add 1 blight
			if(ctx.IsOneOf(Terrain.Jungle,Terrain.Wetland))
				gameState.AddBlight( ctx.Target );

		}

	}
}
