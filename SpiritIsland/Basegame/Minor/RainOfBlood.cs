using SpiritIsland;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	public class RainOfBlood {

		[MinorCard("Rain of Blood", 0, Speed.Slow, "air, water, animal")]
		[FromSacredSite(1,Target.Invaders)]
		static public Task ActAsync(ActionEngine engine, Space target ) {
			int fear = 2;
			if(2<=engine.GameState.InvadersOn( target ).TypeCount(Invader.Town,Invader.City))
				++fear;

			engine.GameState.AddFear( fear );
			return Task.CompletedTask;
		}


	}
}
