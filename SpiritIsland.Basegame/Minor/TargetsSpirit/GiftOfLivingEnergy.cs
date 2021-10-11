using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class GiftOfLivingEnergy {

		[MinorCard( "Gift of Living Energy", 0, Element.Sun, Element.Fire, Element.Plant )]
		[Fast]
		[TargetSpirit]
		public static Task ActAsync( TargetSpiritCtx ctx ) {

			// Target Spirit gains 1 energy
			++ctx.Other.Energy;

			// +1 energy if target spirit is not yourself
			if(ctx.Other != ctx.Self)
				++ctx.Other.Energy;

			// if you have at least 2 SS, target spirit gains +1 energy
			if(2 <= ctx.Self.SacredSites.Count())
				++ctx.Other.Energy;

			return Task.CompletedTask;
		}

	}

}
