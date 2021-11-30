using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class PrepareElement : GrowthActionFactory {
		readonly string context;
		public PrepareElement(string context ) { this.context = context; }

		public override async Task ActivateAsync( SpiritGameStateCtx ctx ) {
			if(ctx.Self is ShiftingMemoryOfAges smoa) 
				await smoa.PrepareElement(context);
		}

	}

}
