using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class MovePresence : GrowthActionFactory, ITrackActionFactory {

		public int Range { get; }

		public MovePresence(int range) {
			this.Range = range;
		}

		public bool RunAfterGrowthResult => true; // might receive additional presence

		public override async Task ActivateAsync( SpiritGameStateCtx ctx) {
			var src = await ctx.Self.Action.Decision( new Decision.Presence.Deployed("Move presence from:", ctx.Self ) );
			var dstOptions = src.Range(Range).Where(s=>s.Terrain!=Terrain.Ocean);
			var dst = await ctx.Self.Action.Decision( new Decision.AdjacentSpace("Move preseence to:", src, Decision.AdjacentDirection.Outgoing, dstOptions));
			ctx.Presence.Move( src, dst );
		}

		public override string Name => $"MovePresence({Range})";
	}


}
