using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class SteamVents {


		[MinorCard("Steam Vents", 1, "fire,air,water,earth")]
		[Fast]
		[FromPresence(0)]
		static public async Task ActAsync(TargetSpaceCtx ctx ) {

			await ctx.SelectActionOption(
				new SpaceAction(
					"Destroy 1 explorer", 
					ctx => ctx.Invaders.Destroy( 1, Invader.Explorer ) 
				),
				new SpaceAction(
					"Destroy 1 town", 
					ctx => ctx.Invaders.Destroy( 1, Invader.Town )
				).FilterOption( ctx.Tokens.Has(Invader.Town) && await ctx.YouHave("3 earth") )
			);
		}

	}
}
