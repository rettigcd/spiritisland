//using System.Collections.Generic;
//using System.Linq;

//namespace SpiritIsland {
//	public class PowerTokenPusher : TokenPusher {

//		public PowerTokenPusher( IMakeGamestateDecisions ctx, Space source )
//			:base(ctx,source)
//		{}

//		protected override IEnumerable<Space> GetAdjacents() {
//			var mapper = SpaceFilter.ForPowers.TerrainMapper;
//			return source.Adjacent.Where(s=>mapper(s) != Terrain.Ocean);
//		}
//	}

//}