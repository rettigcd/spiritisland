using SpiritIsland.Core;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class Drought {

		[MinorCard( "Drought", 1, Speed.Slow, Element.Sun, Element.Fire, Element.Earth )]
		[FromPresence(1)]
		static public async Task Act( ActionEngine engine, Space target ) {
			var grp = engine.GameState.InvadersOn( target );

			// Destory 3 towns.
			grp.DestroyType(Invader.Town,int.MaxValue);

			// 1 damage to each town/city
			while(grp[Invader.Town1] > 0) grp.ApplyDamage(Invader.Town1,1);
			while(grp[Invader.Town] > 0) grp.ApplyDamage( Invader.Town, 1 );

			// add 1 blight
			engine.GameState.AddBlight(target,1);

			// if you have 3 sun, destory 1 city
			if(3 <= engine.Self.Elements[Element.Sun])
				grp.DestroyType(Invader.City,1);

		}

	}
}
