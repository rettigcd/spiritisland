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
			var options = ctx.Tokens.OfAnyType( Invader.City, Invader.Town );
			Token invader = await ctx.Self.Action.Decision( new Decision.InvaderToDowngrade( ctx.Space, options, Present.IfMoreThan1 ) );

			if(invader.Generic == Invader.City) {
				await ReplaceCityWithTown( ctx, invader );
			} else if(invader.Generic == Invader.Town) {
				await ReplaceTownWithExplorer( ctx, invader );
			}
		}

		static Task ReplaceTownWithExplorer( TargetSpaceCtx ctx, Token town ) {
			// deals pre-existing damage to resulting token
			if( town.Health == 1 )
				return ctx.Invaders.Destroy(1,town);
			ctx.Tokens.Adjust( town, -1 );
			ctx.Tokens.Adjust( Invader.Explorer[1], 1 );
			return Task.CompletedTask;
		}

		static Task ReplaceCityWithTown( TargetSpaceCtx ctx, Token city ) {
			// deals pre-existing damage to resulting token
			if( city.Health == 1 )
				return ctx.Invaders.Destroy(1,city);
			ctx.Tokens.Adjust( city, -1 );
			ctx.Tokens.Adjust( Invader.Town[city.Health-1], 1);
			return Task.CompletedTask;
		}
	}
}
