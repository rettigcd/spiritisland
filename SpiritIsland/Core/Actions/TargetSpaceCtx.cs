using System.Threading.Tasks;

namespace SpiritIsland {

	public class TargetSpaceCtx : IMakeGamestateDecisions {

		public TargetSpaceCtx( Spirit self, GameState gameState, Space target ) {
			Self = self;
			GameState = gameState;
			Target = target;
		}

		public Spirit Self { get; }

		public GameState GameState { get; }

		public Space Target { get; }

		public void Deconstruct(out Spirit self, out GameState gameState) {
	        self = Self;
			gameState = GameState;
		}

		public void Deconstruct( out Spirit self, out GameState gameState, out Space target ) {
			self = Self;
			gameState = GameState;
			target = Target;
		}


	}


	static public class IMakeGamestateDecisionsExtension {
		
		// Changable!
		static public Task<Space> TargetSpace( this IMakeGamestateDecisions engine, From sourceEnum, Terrain? sourceTerrain, int range, Target filterEnum )
			=> engine.Self.TargetLandApi.TargetSpace( engine.Self, engine.GameState, sourceEnum, sourceTerrain, range, filterEnum );

		// Not Changable!
		static public InvaderGroup InvadersOn( this IMakeGamestateDecisions engine, Space space )
			=> engine.Self.BuildInvaderGroup( engine.GameState, space );

		static public async Task DamageInvaders( this IMakeGamestateDecisions engine, Space space, int damage ) { // !!! let players choose the item to apply damage to
			if(damage == 0) return;
			await engine.InvadersOn( space ).ApplySmartDamageToGroup( damage );
		}

		static public void AddFear( this IMakeGamestateDecisions engine, int count ) { // need space so we can track fear-space association for bringer
			engine.GameState.AddFearDirect( count );
		}


	}

}