using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	public class RainOfBlood {

		[MinorCard("Rain of Blood", 0, Speed.Slow, "air, water, animal")]
		[FromSacredSite(1,Target.Invaders)]
		static public Task ActAsync(TargetSpaceCtx ctx ) {
			int fear = 2;
			if(2<=ctx.GameState.InvadersOn( ctx.Target ).TownsAndCitiesCount)
				++fear;

			ctx.AddFear( fear );
			return Task.CompletedTask;
		}


	}
}
