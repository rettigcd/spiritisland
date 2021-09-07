using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class PillarOfLivingFlame {

		[MajorCard("Pillar of Living Flame",5,Speed.Slow,Element.Fire)]
		[FromSacredSite(2)]
		static public async Task ActionAsync(TargetSpaceCtx ctx){

			int fear = 3;
			int damage = 5;

			// if you have 4 fire
			if( ctx.Self.Elements.Contains("4 fire" )) {
				fear += 2;
				damage += 5;
			}

			ctx.AddFear( fear );
			await ctx.DamageInvaders( damage );

			// if target is Jungle / Wetland, add 1 blight
			if(ctx.IsOneOf(Terrain.Jungle,Terrain.Wetland))
				ctx.AddBlight();

		}

	}
}
