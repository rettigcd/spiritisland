using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class CastDownIntoTheBrinyDeep {

		[MajorCard( "Cast Down Into the Briny Deep", 9, Speed.Slow, Element.Sun, Element.Moon, Element.Water, Element.Earth )]
		[FromSacredSite( 1, Target.Coastal )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {
			// 6 fear
			ctx.AddFear(6);
			// destroy all invaders
			await ctx.PowerInvaders.DestroyAny(int.MaxValue,Invader.City,Invader.Town,Invader.Explorer);

			// if you have (2 sun, 2 moon, 4 water, 4 earth):
			if(ctx.Self.Elements.Contains( "2 sun,2 moon,4 water,4 earth" )) {
				await DestroyBoard( ctx );

			}
		}

		static async Task DestroyBoard( TargetSpaceCtx ctx ) {
			// destory the board containing target land and everything on that board.
			// All destroyed blight is removed from the game instead of being returned to the blight card.
			await DestoryTokens( ctx );

			throw new System.NotImplementedException("!!!");
		}

		private static async Task DestoryTokens( TargetSpaceCtx ctx ) {
			foreach(var space in ctx.Target.Board.Spaces) {
				// Destory Invaders
				var invaders = ctx.GameState.Invaders.On( space, Cause.Power );
				await invaders.DestroyAny( int.MaxValue, Invader.City, Invader.Town, Invader.Explorer );

				// Destory Dahan
				ctx.GameState.DahanDestroy( space, int.MaxValue, Cause.Power );

				// Destroy everything eles
				var counts = invaders.Counts;
				foreach(var tokens in counts.Keys.ToArray())
					counts[tokens] = 0;
			}
		}
	}

}
