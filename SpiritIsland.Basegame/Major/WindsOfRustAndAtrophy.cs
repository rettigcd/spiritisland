using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class WindsOfRustAndAtrophy {

		[MajorCard("Winds of Rust and Atrophy",3,Speed.Fast,Element.Air,Element.Moon,Element.Animal)]
		[FromSacredSite(3)]
		static public async Task ActAsync(TargetSpaceCtx ctx) {
			await ApplyEffect( ctx );

			// if you have 3 air 3 water 2 animal, repeat this power
			if(ctx.YouHave( "3 air,2 water,2 animal" )) {
				var secondTarget = await ctx.TargetsSpace(From.SacredSite, null, 3,Target.Any);
				await ApplyEffect( secondTarget );
			}
		}

		static async Task ApplyEffect( TargetSpaceCtx ctx ) {
			// 1 fear and defend 6
			ctx.AddFear( 1 );
			ctx.Defend( 6 );

			// replace 1 city with 1 town OR 1 town with 1 explorer
			await Replace.Downgrade( ctx, Invader.City, Invader.Town );
		}

	}
}
