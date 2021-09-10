using System.Collections.Generic;
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

		/// <summary>
		/// Used for Power-targetting, where range sympols appear.
		/// </summary>
		public Task<Space> PowerTargetsSpace( From sourceEnum, Terrain? sourceTerrain, int range, string filterEnum )
			=> Self.PowerApi.TargetsSpace( Self, GameState, sourceEnum, sourceTerrain, range, filterEnum );

	}

}