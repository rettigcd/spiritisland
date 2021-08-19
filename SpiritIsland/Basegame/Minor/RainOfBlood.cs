using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	public class RainOfBlood {

		[MinorCard("Rain of Blood", 0, Speed.Slow, "air, water, animal")]
		[FromSacredSite(1,Target.Invaders)]
		static public Task ActAsync(TargetSpaceCtx ctx ) {
			int fear = (2 <= ctx.InvadersOn.TownsAndCitiesCount)
				? 3 
				: 2;
			ctx.AddFear( fear );
			return Task.CompletedTask;
		}


	}
}
