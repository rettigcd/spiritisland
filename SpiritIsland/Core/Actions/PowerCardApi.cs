using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Core {

	public class PowerCardApi {

		readonly protected ActionEngine engine;

		#region constructor

		public PowerCardApi(ActionEngine engine){
			this.engine = engine;
		}

		#endregion

		#region virutal Target

		public virtual Task<Spirit> TargetSpirit() => engine.SelectSpirit();

		public Task<Space> TargetSpace_Presence(int range)
			=> TargetSpace(engine.Self.Presence,range);

		public Task<Space> TargetSpace_Presence(int range, Func<Space,bool> filter)
			=> TargetSpace(engine.Self.Presence,range,filter);

		public Task<Space> TargetSpace_SacredSite(int range)
			=> TargetSpace(engine.Self.SacredSites,range);
		
		public Task<Space> TargetSpace_SacredSite(int range, Func<Space,bool> filter)
			=> TargetSpace(engine.Self.SacredSites,range,filter);

		public virtual async Task<Space> TargetSpace(IEnumerable<Space> source,int range,Func<Space,bool> filter = null){
			var spaces = source.Range(range);
			if(filter != null)
				spaces = spaces.Where(filter);
			return await engine.SelectSpace("Select target.",spaces);
		}

		#endregion

	}

}
