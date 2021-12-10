﻿using System.Threading.Tasks;

namespace SpiritIsland.PromoPack1 {

	public class GiftOfThePrimordialDeeps {

		[SpiritCard("Gift of the Primordial Deeps", 1, Element.Moon,Element.Earth), Fast, AnotherSpirit]
		public static async Task ActAsync( TargetSpiritCtx ctx ) {
			var otherCtx = ctx.OtherCtx;
			// target spirit gains a minor power.
			var powerCard = (await otherCtx.DrawMinor()).Selected;

			// Target spirit chooses to either:
			var playImmediately = new ActionOption( "Play it immediately by paying its cost", () => otherCtx.Self.PlayCard(powerCard), powerCard.Cost <= otherCtx.Self.Energy );
			var gainMoonAndEarth = new ActionOption( "Gains 1 moon and 1 earth",  () => { var els = otherCtx.Self.Elements; els[Element.Moon]++; els[Element.Earth]++; }  );
			await otherCtx.SelectActionOption( playImmediately, gainMoonAndEarth );

		}
	}


}
