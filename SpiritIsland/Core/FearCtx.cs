using System;
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

		public IEnumerable<SelfCtx> Spirits => this.GameState.Spirits.Select(s=>new SelfCtx(s,GameState,Cause.Fear));

		public InvaderBinding InvadersOn(Space space) => GameState.Invaders.On( space );

		#region Lands

		public IEnumerable<Space> Lands( Func<Space,bool> withCondition ) => GameState.Island.AllSpaces
			.Where(s=> !s.IsOcean)
			.Where(withCondition);

		public IEnumerable<Space> InlandLands => GameState.Island.AllSpaces.Where( s => !s.IsCoastal && !s.IsOcean );

		public bool WithDahanAndExplorers( Space space ) => WithDahan(space) && WithExplorers(space);
		public bool WithDahanAndInvaders( Space space ) => WithDahan( space ) && WithInvaders( space );

		public bool WithExplorers( Space space ) => GameState.Tokens[space].Has( Invader.Explorer );
		public bool WithInvaders( Space space ) => GameState.Tokens[space].HasInvaders();
		public bool WithDahan( Space space ) => GameState.Tokens[space].Dahan.Any;

		public bool WithDahanOrAdjacentTo5( Space space ) => WithDahanOrAdjacentTo(space,5);
		public bool WithDahanOrAdjacentTo3( Space space ) => WithDahanOrAdjacentTo(space,3);
		public bool WithDahanOrAdjacentTo1( Space space ) =>  WithDahanOrAdjacentTo(space,1);
		public bool WithDahanOrAdjacentTo( Space space, int count ) => GameState.DahanOn(space).Any || count <= space.Adjacent.Sum( a => GameState.DahanOn(a).Count );

		#endregion Lands

		// This doesn't work because we don't have the spirit yet
		//public IEnumerable<Space> LandsCtx( Func<Space, bool> withCondition ) => GameState.Island.AllSpaces
		//	.Where( s => s.Terrain != Terrain.Ocean )
		//	.Where( withCondition );

	}

	public static class FearCtxExtensionForBac {

		// Extension to SpiritGameStateCtx
		public static async Task<Space> AddStrifeToOne( this SelfCtx spirit, IEnumerable<Space> options, params TokenClass[] groups ) {
			bool HasInvaders( Space s ) => spirit.Target(s).HasInvaders;
			var space = await spirit.SelectSpace( "Add strife", options.Where( HasInvaders ) );
			if(space != null)
				await space.AddStrife( groups );
			return space?.Space;
		}

		static public IEnumerable<Space> LandsWithStrife(this FearCtx ctx) => ctx.GameState.Island.AllSpaces
			.Where( s => ctx.GameState.Tokens[s].Keys.OfType<StrifedInvader>().Any() );

		static public IEnumerable<Space> LandsWithDisease( this FearCtx ctx ) => ctx.GameState.Island.AllSpaces
			.Where( s => ctx.GameState.Tokens[s].Disease.Any);
		static public IEnumerable<Space> LandsWithBeastDiseaseDahan( this FearCtx ctx ) => ctx.GameState.Island.AllSpaces
			.Where( s => {var tokens = ctx.GameState.Tokens[s]; return tokens.Dahan.Any || tokens.Beasts.Any || tokens.Disease.Any; } );

		static public bool HasBeastOrIsAdjacentToBeast( this FearCtx ctx, Space space ) => space.Range( 1 ).Any( x => ctx.HasBeast(x) );
		static public bool HasBeast( this FearCtx ctx, Space space ) => ctx.GameState.Tokens[space].Beasts.Any;
		static public bool AdjacentToBeast( this FearCtx ctx, Space space ) => space.Adjacent.Any( x => ctx.HasBeast(x) );

		public static IEnumerable<Space> LandsWithBeasts( this FearCtx ctx ) => ctx.GameState.Island.AllSpaces.Where( x => HasBeast(ctx,x) );

		public static IEnumerable<Space> LandsAdjacentToBeasts( this FearCtx ctx ) => ctx.GameState.Island.AllSpaces.Where( x => AdjacentToBeast(ctx,x) );

		public static IEnumerable<Space> LandsWithOrAdjacentToBeasts( this FearCtx ctx ) => ctx.GameState.Island.AllSpaces.Where( x => HasBeastOrIsAdjacentToBeast(ctx, x)  );


	}

}
