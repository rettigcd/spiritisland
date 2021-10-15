using System.Threading.Tasks;

namespace SpiritIsland {

	class TargetLandApi_ExtendRange : TargetLandApi {

		readonly int extension;
		readonly TargetLandApi originalApi;

		public TargetLandApi_ExtendRange( int extension, TargetLandApi originalApi ) {
			this.extension = extension;
			this.originalApi = originalApi;
		}

		public override Task<Space> TargetsSpace( 
			Spirit self, 
			GameState gameState, 
			string prompt,
			From from, 
			Terrain? sourceTerrain, 
			int range, 
			string target
		)
			=> originalApi.TargetsSpace( self, gameState, prompt, from, sourceTerrain, range + extension, target );

	}

}
