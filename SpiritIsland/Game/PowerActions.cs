using System;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland {

	// Called into by Power cards
	// supplied by Spirits
	// allows spirits to swap out the implementation of card actions
	class PowerActions {

		readonly ActionEngine engine;

		#region constructor

		public PowerActions(ActionEngine engine){
			this.engine = engine;
		}

		#endregion

 		#region Select Target

		public Task<Spirit> TargetSpirit()
			=> engine.SelectSpirit(engine.GameState.Spirits);

		public async Task<Space> TargetSpace_Presence(int range){
			return await engine.SelectSpace("Select target.",engine.Self.Presence.Range(range));
		}
		public async Task<Space> TargetSpace_Presence(int range, Func<Space,bool> filter){
			return await engine.SelectSpace("Select target.",engine.Self.Presence.Range(range).Where(filter));
		}

		public async Task<Space> TargetSpace_SacredSite(int range){
			return await engine.SelectSpace("Select target.",engine.Self.SacredSites.Range(range));
		}
		public async Task<Space> TargetSpace_SacredSite(int range, Func<Space,bool> filter){
			return await engine.SelectSpace("Select target.",engine.Self.SacredSites.Range(range).Where(filter));
		}
		#endregion

	}

}