﻿using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class CastDownIntoTheBrinyDeep {

		[MajorCard( "Cast Down Into the Briny Deep", 9, Speed.Slow, Element.Sun, Element.Moon, Element.Water, Element.Earth )]
		[FromSacredSite( 1, Target.Coastal )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {
			// 6 fear
			ctx.AddFear(6);
			// destroy all invaders
			await ctx.Invaders.DestroyAny(int.MaxValue,Invader.City,Invader.Town,Invader.Explorer);

			// if you have (2 sun, 2 moon, 4 water, 4 earth):
			if(ctx.YouHave( "2 sun,2 moon,4 water,4 earth" ))
				await DestroyBoard( ctx );
		}

		static async Task DestroyBoard( TargetSpaceCtx ctx ) {
			// destory the board containing target land and everything on that board.
			// All destroyed blight is removed from the game instead of being returned to the blight card.
			await DestoryTokens( ctx );

			if(!ctx.Self.Text.StartsWith( "Bringer" )) { // !!! Maybe Api shoudl have method called "Destroy Space" or "DestoryBoard"

				// destory presence
				foreach(var spirit in ctx.GameState.Spirits)
				foreach(var p in spirit.Presence.Placed.Where(p=>p.Board==ctx.Space.Board).ToArray() )
					spirit.Presence.Destroy(p);

				// destroy board - spaces
				foreach(var space in ctx.Space.Board.Spaces)
					space.Destroy();

				ctx.GameState.Island.RemoveBoard( ctx.Space.Board );

			}
		}

		static async Task DestoryTokens( TargetSpaceCtx ctx ) {

			foreach(var space in ctx.Space.Board.Spaces) {

				// Destory Invaders
				var invaders = ctx.GameState.Invaders.On( space, Cause.Power );
				await invaders.DestroyAny( int.MaxValue, Invader.City, Invader.Town, Invader.Explorer );

				if(!ctx.Self.Text.StartsWith("Bringer")) { // !!!

					// Destory Dahan
					// !!! needs to go through spirit so Bringer doesn't destory dahan
					await ctx.GameState.DahanDestroy( space, int.MaxValue, Cause.Power );

					// Destroy all other tokens
					var counts = invaders.Tokens;
					foreach(var tokens in counts.Keys.ToArray())
						counts[tokens] = 0;

				}
			}
		}
	}

}
