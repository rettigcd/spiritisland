using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class PrepareElement : GrowthActionFactory, ITrackActionFactory {

		readonly string context;
		public PrepareElement(string context ) { this.context = context; }

		public bool RunAfterGrowthResult => false; // no dependencies

		public override async Task ActivateAsync( SelfCtx ctx ) {
			if(ctx.Self is ShiftingMemoryOfAges smoa) 
				await smoa.PrepareElement(context);
		}

	}

}
