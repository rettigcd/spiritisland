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
				await DestroyBoard( ctx, ctx.Space.Board );
		}

		static async Task DestroyBoard( SelfCtx ctx, Board board ) {
			// destroy the board containing target land and everything on that board.
			// All destroyed blight is removed from the game instead of being returned to the blight card.
			await DestroyTokens( ctx, board );

			if(!ctx.Self.Text.StartsWith( "Bringer" )) { // !!! Maybe Api should have method called "Destroy Space" or "DestroyBoard"

				// destroy presence
				foreach(var spirit in ctx.GameState.Spirits)
					foreach(var p in spirit.Presence.Placed.Where(p=>p.Board==board).ToArray() )
						await spirit.Presence.Destroy(p, ctx.GameState, ActionType.SpiritPower);

				// destroy board - spaces
				foreach(var space in board.Spaces.ToArray())
					board.Remove( space );

				ctx.GameState.Island.RemoveBoard( board );

			}
		}

		static async Task DestroyTokens( SelfCtx ctx, Board board ) {

			foreach(var space in board.Spaces) {

				var spaceCtx = ctx.Target( space );

				// Destroy Invaders
				await spaceCtx.Invaders.DestroyAny( int.MaxValue, Invader.City, Invader.Town, Invader.Explorer );

				// Destroy Dahan
				await spaceCtx.DestroyDahan( int.MaxValue );

				if(!ctx.Self.Text.StartsWith("Bringer")) { // !!!
					// Destroy all other tokens
					var tokens = spaceCtx.Tokens;
					foreach(var token in tokens.Keys.ToArray())
						await tokens.Destroy( token, tokens[token] );
				}
			}
		}
	}

}
