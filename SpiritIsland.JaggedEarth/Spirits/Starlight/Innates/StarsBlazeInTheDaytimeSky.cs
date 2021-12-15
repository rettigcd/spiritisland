using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	[InnatePower("Stars Blaze in the Daytime Sky"), Slow, Yourself]
	class StarsBlazeInTheDaytimeSky{

		[InnateOption("4 sun","3 Fear. Gain 1 Energy. Reclaim up to 1 Power Card from play or your discard pile.")]
		static public async Task Option1( SelfCtx ctx ) {

			// 3 fear
			ctx.AddFear(3);

			// Gain 1 energy
			var spirit = ctx.Self;
			spirit.Energy++;

			// Reclaim up to 1 power card from play or your discard pile
			var cards = spirit.InPlay.Union(spirit.DiscardPile).ToArray();
			if(cards.Length == 0) return;
			var card = await spirit.SelectPowerCard("Reclaim card", cards, CardUse.Reclaim,Present.Always);

			spirit.Reclaim(card);	// reclaim it now

		}

	}


}
