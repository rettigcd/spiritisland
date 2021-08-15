using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class PowerCardApi {

		#region constructor

		public PowerCardApi(){
		}

		#endregion

		#region virutal Target

		public virtual async Task<Space> TargetSpace( ActionEngine engine, IEnumerable<Space> source,int range,Func<Space,bool> filter = null){
            IEnumerable<Space> spaces = source
				.Range( range )
				.Where( s=>s.IsLand );
			if(filter != null)
				spaces = spaces.Where( filter );
			return await engine.SelectSpace( "Select target.", spaces );
		}

		#endregion

	}

}
