using SpiritIsland.Core;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class Drought {

		[MinorCard( "Drought", 1, Speed.Slow, Element.Sun, Element.Fire, Element.Earth )]
		[FromPresence(1)]
		static public Task Act( ActionEngine engine, Space target ) {
			var grp = engine.GameState.InvadersOn( target );

			// Destory 3 towns.
			grp.DestroyType(Invader.Town,int.MaxValue);

			// 1 damage to each town/city - (!! duplicate in Quick the Earths Struggles)
			foreach(var invader in new Invader[] { Invader.Town1, Invader.Town, Invader.City1, Invader.City2, Invader.City })
				while(grp[invader] > 0) 
					grp.ApplyDamageTo1(invader,1);

			// add 1 blight
			engine.GameState.AddBlight(target,1);

			// if you have 3 sun, destory 1 city
			if(3 <= engine.Self.Elements[Element.Sun])
				grp.DestroyType(Invader.City,1);

			return Task.CompletedTask;
		}

	}
}
