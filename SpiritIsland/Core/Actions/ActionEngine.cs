using System;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class ActionEngine {

		public ActionEngine( Spirit self, GameState gameState ) {
			Self = self;
			GameState = gameState;
		}

		public Spirit Self { get; }

		public GameState GameState { get; }

		#region Spirit Configurable

		public Task<Space> TargetSpace( ActionEngine engine, From sourceEnum, int range, Target filterEnum )
			=> Self.TargetLandApi.TargetSpace(engine,sourceEnum,range,filterEnum);

		public Task<Space> TargetSpace( ActionEngine engine, From sourceEnum, Terrain? sourceTerrain, int range, Target filterEnum )
			=> Self.TargetLandApi.TargetSpace( engine, sourceEnum, sourceTerrain, range, filterEnum );

		public InvaderGroup InvadersOn(Space space) => Self.BuildInvaderGroup(GameState,space);

		public async Task DamageInvaders( Space space, int damage ) { // !!! let players choose the item to apply damage to
			if(damage == 0) return;
			await InvadersOn(space).ApplySmartDamageToGroup( damage );
		}

		public void AddFear( int count ) {
			GameState.AddFearDirect( count );
		}

		#endregion

		public void Deconstruct(out Spirit self, out GameState gameState) {
	        self = Self;
			gameState = GameState;
		}

	}

}