using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class MovePresence : GrowthActionFactory, ITrackActionFactory {

		public int Range { get; }

		public MovePresence(int range) {
			this.Range = range;
		}

		public bool RunAfterGrowthResult => true; // might receive additional presence

		public override async Task ActivateAsync( SelfCtx ctx) {
			var src = await ctx.Decision( Select.DeployedPresence.All("Move presence from:", ctx.Self,Present.Always ) );
			var dstOptions = src.Range(Range).Where(s=>!s.IsOcean );
			var dst = await ctx.Decision( Select.Space.ForAdjacent("Move preseence to:", src, Select.AdjacentDirection.Outgoing, dstOptions, Present.Always));
			ctx.Presence.Move( src, dst );
		}

		public override string Name => $"MovePresence({Range})";
	}


}
