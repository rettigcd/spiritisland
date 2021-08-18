using System;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class ActionEngine
//		<TargetType> 
		: IMakeGamestateDecisions {

//		public TargetType Target { get; private set; }

		public ActionEngine( Spirit self, GameState gameState
//			, TargetType target 
		) {
			Self = self;
			GameState = gameState;
//			Target = target;
		}

		public Spirit Self { get; }

		public GameState GameState { get; }

		public Task<Space> TargetSpace( From sourceEnum, int range, Target filterEnum )
			=> Self.TargetLandApi.TargetSpace(Self,GameState,sourceEnum,range,filterEnum);

		public Task<Space> TargetSpace( From sourceEnum, Terrain? sourceTerrain, int range, Target filterEnum )
			=> Self.TargetLandApi.TargetSpace( Self, GameState, sourceEnum, sourceTerrain, range, filterEnum );

		public InvaderGroup InvadersOn(Space space) 
			=> Self.BuildInvaderGroup(GameState,space);

		public async Task DamageInvaders( Space space, int damage ) { // !!! let players choose the item to apply damage to
			if(damage == 0) return;
			await InvadersOn(space).ApplySmartDamageToGroup( damage );
		}

		public void AddFear( int count ) { // need space so we can track fear-space association for bringer
			GameState.AddFearDirect( count );
		}

		public void Deconstruct(out Spirit self, out GameState gameState) {
	        self = Self;
			gameState = GameState;
		}

	}

}