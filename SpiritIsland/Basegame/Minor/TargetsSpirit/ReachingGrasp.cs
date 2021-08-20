﻿using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class ReachingGrasp {

		[MinorCard( "Reaching Grasp", 0, Speed.Fast, Element.Sun, Element.Air, Element.Water)]
		[TargetSpirit]
		static public Task Act( TargetSpiritCtx ctx ) {
			// target spirit gets +2 range with all their Powers
			var original = ctx.Target.PowerCardApi;
			ctx.Target.PowerCardApi = new ExtendRange( 2, original );

			Task cleanup(GameState _ ) {
				ctx.Target.PowerCardApi = original;
				return Task.CompletedTask;
			}
			ctx.GameState.TimePasses_ThisRound.Push(cleanup);
			return Task.CompletedTask;
		}

		class ExtendRange : TargetLandApi {
			readonly int extension;
			readonly TargetLandApi original;
			public ExtendRange(int extension,TargetLandApi original ) {
				this.extension = extension;
				this.original = original;
			}

			public override Task<Space> TargetSpace( Spirit self, GameState gameState, From from, Terrain? sourceTerrain, int range, Target target )
				=> original.TargetSpace( self, gameState, from, sourceTerrain, range + extension, target );

		}

	}
}
