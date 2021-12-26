using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	class Discord : IFearOptions {

		public const string Name = "Discord";
		string IFearOptions.Name => Name;

		[FearLevel( 1, "Each player adds 1 strife in a different land with at least 2 invaders" )]
		public async Task Level1( FearCtx ctx ) {
			var options = LandsWith2Invaders( ctx );

			// each player adds 1 strife in a different land with at least 2 invaders
			foreach(SelfCtx spirit in ctx.Spirits)
				options.Remove( await spirit.AddStrifeToOne(options) );

		}

		[FearLevel( 2, "Each player adds 1 strife in a different land with at least 2 invaders. Then each invader takes 1 damage per strife it has." )]
		public async Task Level2( FearCtx ctx ) {
			var options = LandsWith2Invaders( ctx );

			// each player adds 1 strife in a different land with at least 2 invaders
			foreach(SelfCtx spirit in ctx.Spirits)
				options.Remove( await spirit.AddStrifeToOne( options ) );

			// Then each invader takes 1 damage per strife it has.
			StrifedRavage.StrifedInvadersLoseHealthPerStrife( ctx );
		}

		[FearLevel( 3, "each player adds 1 strife in a different land with at least 2 invaders. Then, each invader with strife deals damage to other invaders in that land." )]
		public async Task Level3( FearCtx ctx ) {
			var options = LandsWith2Invaders( ctx );

			// each player adds 1 strife in a different land with at least 2 invaders.
			foreach(SelfCtx spirit in ctx.Spirits) {
				var spaceCtx = await spirit.SelectSpace( "Add strife", options );
				if(spaceCtx != null) {
					await spaceCtx.AddStrife();
					options.Remove( spaceCtx.Space );

					// Then, each invader with strife deals damage to other invaders in that land.
					int damage = StrifedRavage.DamageFromStrifedInvaders( spaceCtx.Tokens );
					await StrifedRavage.DamageUnStriffed( spaceCtx, damage );
				}
			}
		}

		static List<Space> LandsWith2Invaders( FearCtx ctx ) {
			return ctx.GameState.Island.AllSpaces
				.Where( s => 2 <= ctx.GameState.Tokens[s].InvaderTotal() ).ToList();
		}

	}

}
