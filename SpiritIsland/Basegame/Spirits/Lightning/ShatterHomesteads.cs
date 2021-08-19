using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {

	public class ShatterHomesteads {
		public const string Name = "Shatter Homesteads";

		[SpiritCard(ShatterHomesteads.Name,2,Speed.Slow,Element.Fire,Element.Air)]
		[FromSacredSite(2)]
		static public async Task Act(TargetSpaceCtx ctx){
			// Destroy 1 town
			await ctx.InvadersOn(ctx.Target).Destroy( Invader.Town, 1 );

			// 1 fear
			ctx.AddFear(1);
		}

	}

}
