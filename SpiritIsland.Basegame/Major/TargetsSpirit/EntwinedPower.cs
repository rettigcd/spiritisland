﻿using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class EntwinedPower {


		[MajorCard( "Entwined Power", 2, Element.Moon, Element.Water, Element.Plant ),Fast,AnotherSpirit]
		static public async Task ActAsync( TargetSpiritCtx ctx ) {

			// You and other spirit share presence for targeting
			if( ctx.Self != ctx.Other) {
				ctx.GameState.TimePasses_ThisRound.Push( new SourceCalcRestorer( ctx.Self ).Restore );
				ctx.GameState.TimePasses_ThisRound.Push( new SourceCalcRestorer( ctx.Other ).Restore );
				_ = new EntwinedPresenceSource( ctx.Self, ctx.Other ); // auto-binds to spirits
			}

			// Target spirit gains a power Card.
			var result = await ctx.OtherCtx.Draw();
			// You gain one of the power Cards they did not keep.
			await DrawFromDeck.TakeCard( ctx.Self, result.Rejected.ToList() );

			// if you have 2 water, 4 plant, 
			if(await ctx.YouHave("2 water,4 plant")) {
				// you and target spirit each gain 3 energy
				ctx.Self.Energy += 3;
				ctx.Other.Energy += 3;
				// may gift the other 1 power from hand.
				await GiftCardToSpirit( ctx.Self, ctx.Other );
				await GiftCardToSpirit( ctx.Other, ctx.Self );
			}
		}

		static async Task GiftCardToSpirit( Spirit src, Spirit dst ) {
			var myGift = await src.SelectPowerCard( "Select gift for " + dst.Text, src.Hand.ToArray(), CardUse.Gift, Present.Done );
			if(myGift != null) {
				dst.Hand.Add( myGift );
				src.Hand.Remove( myGift );
			}
		}
	}
}

