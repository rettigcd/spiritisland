using System.Threading.Tasks;

namespace SpiritIsland {

	public class Reclaim1 : GrowthActionFactory, ITrackActionFactory {

		public const string Prompt = "Select card to reclaim.";

		public bool RunAfterGrowthResult => true; // If user Reclaims All during growth, no need to reclaim single

		public override async Task ActivateAsync( SelfCtx ctx ) {
			var self = ctx.Self;
			if(self.DiscardPile.Count == 0) return;

			PowerCard cardToReclaim = await ctx.Self.SelectPowerCard( Prompt, self.DiscardPile, CardUse.Reclaim, Present.Always );
			if(cardToReclaim != null)
				self.Reclaim( cardToReclaim );

		}

		public override string Name => "Reclaim(1)";

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
