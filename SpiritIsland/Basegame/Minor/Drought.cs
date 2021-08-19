using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class Drought {

		[MinorCard( "Drought", 1, Speed.Slow, Element.Sun, Element.Fire, Element.Earth )]
		[FromPresence(1)]
		static public async Task Act( TargetSpaceCtx ctx ) {
			var target = ctx.Target;
			var grp = ctx.InvadersOn( target );

			// Destory 3 towns.
			await grp.Destroy(Invader.Town, int.MaxValue);

			// 1 damage to each town/city - (!! duplicate in Quick the Earths Struggles)
			await grp.ApplyDamageToEach(1,Invader.Town,Invader.City);

			// add 1 blight
			ctx.GameState.AddBlight(target,1);

			// if you have 3 sun, destory 1 city
			if(3 <= ctx.Self.Elements[Element.Sun])
				await grp.Destroy(Invader.City, 1);
		}

	}
}
