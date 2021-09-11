using System.Threading.Tasks;

namespace SpiritIsland {

	public class PowerCtx : SpiritGameStateCtx {

		#region constructor

		public PowerCtx( Spirit self, GameState gameState ) : base( self, gameState ) {}

		#endregion

		protected override SpaceFilter SpaceFilter => SpaceFilter.ForPowers;

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
		public async Task<TargetSpaceCtx> TargetsSpace( From sourceEnum, Terrain? sourceTerrain, int range, string filterEnum ) {
			var space = await Self.PowerApi.TargetsSpace( Self, GameState, sourceEnum, sourceTerrain, range, filterEnum );
			return new TargetSpaceCtx( Self, GameState, space );
		}

		public TargetSpaceCtx TargetSpace( Space space ) => new TargetSpaceCtx( Self, GameState, space );

		// Don't put this in SpiritGameStateCtx because it returns a derivitive of PowerCtx
		public async Task<TargetSpaceCtx> TargetLandWithPresence( string prompt ) {
			var space = await Self.Action.Decision( new Decision.PresenceDeployed( prompt, Self ) );
			return new TargetSpaceCtx( Self, GameState, space );
		}

	}

}