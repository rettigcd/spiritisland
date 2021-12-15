using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	public class SleepAndNeverWaken {

		[MajorCard("Sleep and Never Waken",3,Element.Moon,Element.Air,Element.Earth,Element.Animal), Fast, FromPresenceIn(2,Terrain.Sand)]
		public static async Task ActAsync(TargetSpaceCtx ctx ) {
			// invaders skip all actions in target land.
			ctx.GameState.SkipAllInvaderActions();

			// remove up to 2 explorer.
			int removed = await RemoveExploreres( ctx, 2, ctx.Space );

			// if you have 3 moon 2 air 2 animal:  Remove up to 6 explorer from among your lands.
			if( await ctx.YouHave( "3 moon,2 air,2 animal") )
				removed += await RemoveExploreres( ctx, 6, ctx.Self.Presence.Spaces.ToArray() );

			// 1 fear per 2 explorer this Power Removes.
			ctx.AddFear( removed / 2 );
		}

		static async Task<int> RemoveExploreres( TargetSpaceCtx ctx, int count, params Space[] fromSpaces ) {

			SpaceToken[] CalcOptions() => fromSpaces
				.SelectMany(
					space => ctx.GameState.Tokens[space].OfType(Invader.Explorer)
						.Select( t=>new SpaceToken(ctx.Space,t) )
				)
				.ToArray();

			int countRemoved = 0;

			SpaceToken[] options;
			while( count-- > 0 
				&& (options=CalcOptions()).Length > 0 
			) {
				var token = await ctx.Decision( new Select.TokenFromManySpaces($"Select Explorer to remove. ({count+1} remaining)", options, Present.Done));
				if(token == null ) break;
				await ctx.GameState.Tokens[token.Space].Remove(token.Token,1);
				++countRemoved;
			}

			return countRemoved;

		}

	}


}
