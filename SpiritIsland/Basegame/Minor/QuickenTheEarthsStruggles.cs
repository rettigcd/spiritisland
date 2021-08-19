using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	class QuickenTheEarthsStruggles {

		[MinorCard( "Quicken the Earths Struggles", 1, Speed.Fast, "moon, fire, earth, animal" )]
		[FromSacredSite( 0 )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {
			var grp = ctx.InvadersOn( ctx.Target ); // !!!

			if(await ctx.Self.SelectFirstText("Select power option", "1 damage to each town/city","defend 10"))
				// 1 damage to each town/city
				await grp.ApplyDamageToEach(1,Invader.City,Invader.Town);
			else
				// defend 10
				ctx.GameState.Defend(ctx.Target,10);
		}


	}
}
