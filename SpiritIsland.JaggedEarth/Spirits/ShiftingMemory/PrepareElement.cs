using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class PrepareElement : GrowthActionFactory {

		public override async Task ActivateAsync( SpiritGameStateCtx ctx ) {
			if(ctx.Self is ShiftingMemoryOfAges smoa) 
				await smoa.PrepareElement();
		}

	}

}
