using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	class QuickenTheEarthsStruggles {

		[MinorCard( "Quicken the Earths Struggles", 1, Speed.Fast, "moon, fire, earth, animal" )]
		[FromSacredSite( 0 )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			if(await ctx.Self.UserSelectsFirstText("Select power option", "1 damage to each town/city","defend 10"))
				// 1 damage to each town/city
				await ctx.PowerInvaders.ApplyDamageToEach(1,Invader.City,Invader.Town);
			else
				// defend 10
				ctx.Defend(10);
		}


	}
}
