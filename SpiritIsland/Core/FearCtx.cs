using System;
using System.Collections.Generic;
using System.Linq;

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

		public IEnumerable<Space> InlandLands                 => GameState.Island.AllSpaces.Where( s => !s.IsCostal && s.Terrain != Terrain.Ocean );

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

	//public class With {
	//	static public bool DahanAndExplorers( TargetSpaceCtx ctx ) => ctx.HasDahan && Explorers( ctx );
	//	static public bool DahanAndInvaders( TargetSpaceCtx ctx ) => ctx.HasDahan && Invaders( ctx );

	//	static public bool Explorers( TargetSpaceCtx ctx ) => ctx.Tokens.Has( Invader.Explorer );
	//	static public bool Invaders( TargetSpaceCtx ctx ) => ctx.Tokens.HasInvaders();
	//	static public bool Dahan( TargetSpaceCtx ctx ) => ctx.HasDahan;

	//	static public bool DahanOrAdjacentTo5( TargetSpaceCtx ctx ) => ctx.HasDahan || ctx.Space.Adjacent.Any( a => 5 <= ctx.GameState.Tokens[a].Sum( TokenType.Dahan ) );
	//	static public bool DahanOrAdjacentTo3( TargetSpaceCtx ctx ) => ctx.HasDahan || ctx.Space.Adjacent.Any( a => 3 <= ctx.GameState.Tokens[a].Sum( TokenType.Dahan ) );
	//	static public bool DahanOrAdjacentTo1( TargetSpaceCtx ctx ) => ctx.HasDahan || ctx.Space.Adjacent.Any( a => 1 <= ctx.GameState.Tokens[a].Sum( TokenType.Dahan ) );
	//}


}
