using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	public class PlowsShatterOnRockyGround {

		[SpiritCard("Plows Shatter on Rocky Ground",2,Element.Earth), Slow, FromPresence(1)]
		static public Task ActAsync(TargetSpaceCtx ctx ) {
			return ctx.SelectActionOption(
				new ActionOption("1 damage to each town / city. Push 1 town.", async () => {
					// 1 damage to each town / city.
					await ctx.Invaders.ApplyDamageToEach(1,Invader.Town,Invader.City);
					// push up to 1 town.
					await ctx.PushUpTo(1,Invader.Town);
				}),
				new ActionOption("Destroy 1 town", ()=>ctx.Invaders.Destroy(1,Invader.Town))
			);
		}

	}

}
