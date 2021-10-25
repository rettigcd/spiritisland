using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class Drought {

		[MinorCard( "Drought", 1, Element.Sun, Element.Fire, Element.Earth )]
		[Slow]
		[FromPresence(1)]
		static public async Task Act( TargetSpaceCtx ctx ) {
			var invaders = ctx.Invaders;

			// Destory 3 towns.
			await ctx.Invaders.Destroy( 3, Invader.Town );

			// 1 damage to each town/city
			await ctx.DamageEachInvader( 1, Invader.Town, Invader.City );

			// add 1 blight
			await ctx.AddBlight();

			// if you have 3 sun, destory 1 city
			if( ctx.YouHave("3 sun") )
				await ctx.Invaders.Destroy( 1, Invader.City );
		}

	}
}
