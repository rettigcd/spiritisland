﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class FearCtx {

		public readonly GameState GameState;

		#region constructor

		public FearCtx(GameState gs) {
			this.GameState = gs;
		}

		#endregion constructor

		public IEnumerable<SpiritGameStateCtx> Spirits => this.GameState.Spirits.Select(s=>new SpiritGameStateCtx(s,GameState,Cause.Fear));

		public InvaderGroup InvadersOn(Space space) => GameState.Invaders.On(space,Cause.Fear);

		#region Lands

		public IEnumerable<Space> Lands( Func<Space,bool> withCondition ) => GameState.Island.AllSpaces
			.Where(s=>s.Terrain!=Terrain.Ocean)
			.Where(withCondition);

		public IEnumerable<Space> InlandLands                 => GameState.Island.AllSpaces.Where( s => !s.IsCoastal && s.Terrain != Terrain.Ocean );

		public bool WithDahanAndExplorers( Space space ) => WithDahan(space) && WithExplorers(space);
		public bool WithDahanAndInvaders( Space space ) => WithDahan( space ) && WithInvaders( space );

		public bool WithExplorers( Space space ) => GameState.Tokens[space].Has( Invader.Explorer );
		public bool WithInvaders( Space space ) => GameState.Tokens[space].HasInvaders();
		public bool WithDahan( Space space ) => GameState.Tokens[space].Has( TokenType.Dahan );
		public bool WithDahanOrAdjacentTo5( Space space ) => GameState.DahanIsOn(space) || space.Adjacent.Any( a => GameState.Tokens[a].Sum(TokenType.Dahan)>5 );
		public bool WithDahanOrAdjacentTo3( Space space ) => GameState.DahanIsOn( space ) || space.Adjacent.Any( a => GameState.Tokens[a].Sum( TokenType.Dahan ) > 5 );
		public bool WithDahanOrAdjacentTo1( Space space ) => GameState.DahanIsOn( space ) || space.Adjacent.Any( a => GameState.Tokens[a].Sum( TokenType.Dahan ) > 5 );

		#endregion Lands

		// This doesn't work because we don't have the spirit yet
		//public IEnumerable<Space> LandsCtx( Func<Space, bool> withCondition ) => GameState.Island.AllSpaces
		//	.Where( s => s.Terrain != Terrain.Ocean )
		//	.Where( withCondition );

	}

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

		static public IEnumerable<Space> LandsWithDisease( this FearCtx ctx ) => ctx.GameState.Island.AllSpaces.Where( s => ctx.GameState.Tokens[s].Has( TokenType.Disease ) );
		static public IEnumerable<Space> LandsWithBeastDiseaseDahan( this FearCtx ctx ) => ctx.GameState.Island.AllSpaces.Where( s => ctx.GameState.Tokens[s].HasAny( TokenType.Beast.Generic, TokenType.Disease.Generic, TokenType.Dahan ) );

		static public bool HasBeastOrIsAdjacentToBeast( this FearCtx ctx, Space space ) => space.Range( 1 ).Any( x => ctx.HasBeast(x) );
		static public bool HasBeast( this FearCtx ctx, Space space ) => ctx.GameState.Tokens[space].Beasts.Any;
		static public bool AdjacentToBeast( this FearCtx ctx, Space space ) => space.Adjacent.Any( x => ctx.HasBeast(x) );

		public static IEnumerable<Space> LandsWithBeasts( this FearCtx ctx ) => ctx.GameState.Island.AllSpaces.Where( x => HasBeast(ctx,x) );

		public static IEnumerable<Space> LandsAdjacentToBeasts( this FearCtx ctx ) => ctx.GameState.Island.AllSpaces.Where( x => AdjacentToBeast(ctx,x) );

		public static IEnumerable<Space> LandsWithOrAdjacentToBeasts( this FearCtx ctx ) => ctx.GameState.Island.AllSpaces.Where( x => HasBeastOrIsAdjacentToBeast(ctx, x)  );


	}

}
