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

		public virtual async Task<Space> TargetSpace(IEnumerable<Space> source,int range,Func<Space,bool> filter = null){
			var spaces = source.Range( range );
			if(filter != null)
				spaces = spaces.Where( filter );
			return await engine.SelectSpace( "Select target.", spaces );
		}

		#endregion

	}

}
