﻿using System.Threading.Tasks;

namespace SpiritIsland {

	public class Reclaim1 : GrowthActionFactory {

		public override async Task ActivateAsync( SpiritGameStateCtx ctx ) {
			var self = ctx.Self;
			if(self.DiscardPile.Count == 0) return;

			PowerCard cardToReclaim = await ctx.Self.SelectPowerCard( "Select card to reclaim", self.DiscardPile, CardUse.Reclaim, Present.Always );
			if(cardToReclaim != null && self.DiscardPile.Contains( cardToReclaim )) {
				self.DiscardPile.Remove( cardToReclaim );
				self.Hand.Add( cardToReclaim );
			}

		}

		public override string Name => "Reclaim(1)";

	}

}
