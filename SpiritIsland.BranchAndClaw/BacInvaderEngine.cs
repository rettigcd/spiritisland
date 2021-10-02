using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {
	class BacInvaderEngine : InvaderEngine {

		public BacInvaderEngine(GameState gs ):base(gs) {}

		#region explore

		public override async Task ExploresSpace( Space space ) {
			// only gets called when explorer is actually going to explore
			var wilds = gs.Tokens[space].Wilds();
			if(wilds == 0)
				await base.ExploresSpace( space );
			else
				wilds.Count--;
		}

		#endregion

		#region build

		public override async Task<string> Build( TokenCountDictionary tokens, BuildingEventArgs.BuildType buildType ) {
			// ! Instead of overriding this, we could handle the pre-build event
			var disease = tokens.Disease();
			if(disease.Any) {
				disease.Count--;
				return tokens.Space.Label +" build stopped by disease";
			}
			return await base.Build( tokens, buildType );
		}

		#endregion

		#region Ravage

		public override async Task<string> RavageSpace( InvaderGroup grp ) {
			var cfg = gs.GetRavageConfiguration( grp.Space );
			var eng = new RavageEngineWithStrife( gs, grp, cfg );
			await eng.Exec();
			return grp.Space.Label + ": " + eng.log.Join( "  " );
		}

		#endregion
	}

}
