using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class CastDownIntoTheBrinyDeep {

		[MajorCard( "Cast Down Into the Briny Deep", 9, Element.Sun, Element.Moon, Element.Water, Element.Earth )]
		[Slow]
		[FromSacredSite( 1, Target.Coastal )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {
			// 6 fear
			ctx.AddFear(6);
			// destroy all invaders
			await ctx.Invaders.DestroyAny(int.MaxValue,Invader.City,Invader.Town,Invader.Explorer);

			// if you have (2 sun, 2 moon, 4 water, 4 earth):
			if(await ctx.YouHave("2 sun,2 moon,4 water,4 earth"))
				await DestroyBoard( ctx );
		}

		static async Task DestroyBoard( TargetSpaceCtx ctx ) {
			// destory the board containing target land and everything on that board.
			// All destroyed blight is removed from the game instead of being returned to the blight card.
			await DestoryTokens( ctx );

			if(!ctx.Self.Text.StartsWith( "Bringer" )) { // !!! Maybe Api should have method called "Destroy Space" or "DestoryBoard"

				// destory presence
				foreach(var spirit in ctx.GameState.Spirits)
					foreach(var p in spirit.Presence.Placed.Where(p=>p.Board==ctx.Space.Board).ToArray() )
						await spirit.Presence.Destroy(p, ctx.GameState);

				// destroy board - spaces
				foreach(var space in ctx.Space.Board.Spaces)
					space.Destroy();

				ctx.GameState.Island.RemoveBoard( ctx.Space.Board );

			}
		}

		static async Task DestoryTokens( TargetSpaceCtx ctx ) {

			foreach(var space in ctx.Space.Board.Spaces) {

				var spaceCtx = ctx.Target( space );

				// Destory Invaders
				await spaceCtx.Invaders.DestroyAny( int.MaxValue, Invader.City, Invader.Town, Invader.Explorer );

				// Destory Dahan
				await spaceCtx.DestroyDahan( int.MaxValue );

				if(!ctx.Self.Text.StartsWith("Bringer")) { // !!!
					// Destroy all other tokens
					var counts = spaceCtx.Tokens;
					foreach(var tokens in counts.Keys.ToArray())
						counts[tokens] = 0;
				}
			}
		}
	}

}
