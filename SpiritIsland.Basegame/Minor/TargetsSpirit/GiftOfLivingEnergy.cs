using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	public class GiftOfLivingEnergy {
		[MinorCard( "Gift of Living Energy", 0, Speed.Fast, Element.Air, Element.Fire, Element.Plant )]
		[TargetSpirit]
		public static Task ActAsync( TargetSpiritCtx ctx ) {
			++ctx.Target.Energy;
			if(ctx.Target != ctx.Self)
				++ctx.Target.Energy;
			if(ctx.Self.SacredSites.Count() >= 2)
				++ctx.Target.Energy;
			return Task.CompletedTask;
		}
	}


}
