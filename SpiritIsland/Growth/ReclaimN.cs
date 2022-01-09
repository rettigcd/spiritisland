using System.Threading.Tasks;

namespace SpiritIsland {

	public class ReclaimN : GrowthActionFactory, ITrackActionFactory {

		public const string Prompt = "Select card to reclaim.";

		public bool RunAfterGrowthResult => true; // If user Reclaims All during growth, no need to reclaim single

		public ReclaimN(int count=1) {
			this.count = count;
		}

		public override async Task ActivateAsync( SelfCtx ctx ) {
			if(ctx.Self.DiscardPile.Count == 0) return;

			int reclaimCount = count;

			while(reclaimCount-- > 0)
				await ctx.Self.Reclaim1FromDiscard();

		}

		readonly int count;

		public override string Name => $"Reclaim({count})";

	}

	public class ReclaimHalf : GrowthActionFactory {

		public override async Task ActivateAsync( SelfCtx ctx ) {
			int reclaimCount = (ctx.Self.DiscardPile.Count + 1) / 2; // round up
			if(reclaimCount == 0) return;

			while(reclaimCount-- > 0)
				await ctx.Self.Reclaim1FromDiscard();

		}

		public override string Name => "Reclaim(1/2)";

	}

}
