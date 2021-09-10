﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class PowerCtx : SpiritGameStateCtx {

		#region constructor

		public PowerCtx( Spirit self, GameState gameState ) : base( self, gameState ) {}

		#endregion

		public override IEnumerable<Space> AdjacentTo( Space source )
			=> source.Adjacent.Where( x => SpaceFilter.ForPowers.TerrainMapper( x ) != Terrain.Ocean );

		// Use this to create Power-Damage and Power-Fear
		public InvaderGroup InvadersOn( Space target )
			=> Self.BuildInvaderGroupForPowers( GameState, target );

		public async Task DamageInvaders( Space space, int damage ) {
			if(damage == 0) return;
			await InvadersOn( space ).ApplySmartDamageToGroup( damage );
		}

		/// <summary> changes Cause to .Power </summary>
		public override void AddFear( int count ) { // need space so we can track fear-space association for bringer
			GameState.Fear.AddDirect( new FearArgs { count = count, cause = Cause.Power, space = null } );
		}

		public bool YouHave(string elementString ) => Self.Elements.Contains( elementString );

		/// <summary>
		/// Used for Power-targetting, where range sympols appear.
		/// </summary>
		public async Task<TargetSpaceCtx> TargetsSpace( From sourceEnum, Terrain? sourceTerrain, int range, string filterEnum ) {
			var space = await Self.PowerApi.TargetsSpace( Self, GameState, sourceEnum, sourceTerrain, range, filterEnum );
			return new TargetSpaceCtx( Self, GameState, space );
		}

		public TargetSpaceCtx TargetSpace( Space space ) => new TargetSpaceCtx( Self, GameState, space );

		public async Task<TargetSpaceCtx> TargetLandWithPresence( string prompt ) {
			var space = await Self.Action.Decide( new SelectDeployedPresence( prompt, Self ) );
			return new TargetSpaceCtx( Self, GameState, space );
		}

	}

}