using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public static class FearCtxExtensionForBac {

		public static void StrifedInvadersLoseHealthPerStrife( this FearCtx ctx ) {
			// !!! We need a reset other than end-of-round when Silent Shroud is in play
			foreach(var space in ctx.GameState.Island.AllSpaces) {
				var tokens = ctx.InvadersOn( space ).Tokens;
				var strifedInvaders = tokens.Invaders()
					.OfType<StrifedInvader>()
					.Where( x => x.Health > 1 )
					.OrderBy( x => x.Health ); // get the lowest ones first so we can reduce without them cascading
				foreach(StrifedInvader strifedInvader in strifedInvaders) {
					var newInvader = strifedInvader.ResultingDamagedInvader( strifedInvader.StrifeCount );
					if(newInvader.Health > 0) {
						tokens[newInvader] = tokens[strifedInvader];
						tokens[strifedInvader] = 0;
					}
				}
			}
		}

		// Extension to SpiritGameStateCtx
		public static async Task<Space> AddStrifeToOne( this SpiritGameStateCtx spirit, IEnumerable<Space> options, params TokenGroup[] groups ) {
			bool HasInvaders( Space s ) => spirit.Target(s).HasInvaders;
			var space = await spirit.SelectSpace( "Add strife", options.Where( HasInvaders ) );
			if(space != null)
				await space.AddStrife( groups );
			return space?.Space;
		}

		static public IEnumerable<Space> LandsWithStrife(this FearCtx ctx)             => ctx.GameState.Island.AllSpaces.Where( s => ctx.GameState.Tokens[s].Keys.OfType<StrifedInvader>().Any() );

		static public IEnumerable<Space> LandsWithDisease( this FearCtx ctx ) => ctx.GameState.Island.AllSpaces.Where( s => ctx.GameState.Tokens[s].Has( BacTokens.Disease ) );
		static public IEnumerable<Space> LandsWithBeastDiseaseDahan( this FearCtx ctx ) => ctx.GameState.Island.AllSpaces.Where( s => ctx.GameState.Tokens[s].HasAny( BacTokens.Beast.Generic, BacTokens.Disease.Generic, TokenType.Dahan ) );

		static public bool HasBeastOrIsAdjacentToBeast( this FearCtx ctx, Space space ) => space.Range( 1 ).Any( x => ctx.HasBeast(x) );
		static public bool HasBeast( this FearCtx ctx, Space space ) => ctx.GameState.Tokens[space].Beasts().Any;
		static public bool AdjacentToBeast( this FearCtx ctx, Space space ) => space.Adjacent.Any( x => ctx.HasBeast(x) );

		public static IEnumerable<Space> LandsWithBeasts( this FearCtx ctx ) => ctx.GameState.Island.AllSpaces.Where( x => HasBeast(ctx,x) );

		public static IEnumerable<Space> LandsAdjacentToBeasts( this FearCtx ctx ) => ctx.GameState.Island.AllSpaces.Where( x => AdjacentToBeast(ctx,x) );

		public static IEnumerable<Space> LandsWithOrAdjacentToBeasts( this FearCtx ctx ) => ctx.GameState.Island.AllSpaces.Where( x => HasBeastOrIsAdjacentToBeast(ctx, x)  );


	}

}
