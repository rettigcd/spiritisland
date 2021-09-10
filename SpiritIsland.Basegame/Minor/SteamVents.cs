using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class SteamVents {


		[MinorCard("Steam Vents", 1, Speed.Fast, "fire,air,water,earth")]
		[FromPresence(0)]
		static public async Task ActAsync(TargetSpaceCtx ctx ) {

			await ctx.SelectActionOption(
				new ActionOption(
					"Destory 1 explorer", 
					() => ctx.Invaders.Destroy( 1, Invader.Explorer ) 
				),
				new ActionOption(
					"Destory 1 town", 
					() => ctx.Invaders.Destroy( 1, Invader.Town ), 
					ctx.Tokens.Has(Invader.Town) && ctx.YouHave("3 earth")
				)
			);
		}

	}
}
