using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class ReachingGrasp {

	[MinorCard( "Reaching Grasp", 0, Speed.Fast, Element.Sun, Element.Air, Element.Water )]
	[TargetSpirit]
	static public Task Act( TargetSpiritCtx ctx ) {
		// target spirit gets +2 range with all their Powers
		var original = ctx.Target.PowerApi;

		Task cleanup( GameState _ ) {
			ctx.Target.PowerApi = original;
			return Task.CompletedTask;
		}
		ctx.GameState.TimePasses_ThisRound.Push( cleanup );
		return Task.CompletedTask;
	}

	class ExtendRange : TargetLandApi {

		readonly int extension;
		readonly TargetLandApi originalApi;

		public ExtendRange( int extension, TargetLandApi originalApi ) {
			this.extension = extension;
			this.originalApi = originalApi;
		}

		public override Task<Space> TargetsSpace( 
			Spirit self, 
			GameState gameState, 
			From from, 
			Terrain? sourceTerrain, 
			int range, 
			Target target
		)
			=> originalApi.TargetsSpace( self, gameState, from, sourceTerrain, range + extension, target );

	}

}


}
