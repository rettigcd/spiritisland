﻿using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class EntwinedPower {


		[MajorCard( "Entwined Power", 2, Speed.Fast, Element.Moon, Element.Water, Element.Plant )]
		[TargetSpirit]
		static public async Task ActAsync( TargetSpiritCtx ctx ) {
			var gs = ctx.GameState;

			// You and other spirit share presence for targeting
			if( ctx.Self != ctx.Other) {
				TargetLandApi.ScheduleRestore( ctx );
				TargetLandApi.ScheduleRestore( ctx.OtherCtx );
				_ = new SharedPresenceTargeting( ctx.Self, ctx.Other ); // auto-binds to spirits
			}

			// Target spirit gains a power Card.
			await ctx.Other.Draw( gs, ( cards ) => {
				// You gain one of the power Cards they did not keep.
				return DrawFromDeck.TakeCard( ctx.Self, cards );
			} );

			// if you have 2 water, 4 plant, 
			if(ctx.YouHave( "2 water,4 plant" )) {
				// you and target spirit each gain 3 energy
				ctx.Self.Energy += 3;
				ctx.Other.Energy += 3;
				// may gift the other 1 power from hand.
				await GiftCardToSpirit( ctx.Self, ctx.Other );
				await GiftCardToSpirit( ctx.Other, ctx.Self );
			}
		}

		static async Task GiftCardToSpirit( Spirit src, Spirit dst ) {
			var myGift = (PowerCard)await src.Select( "Select gift for " + dst.Text, src.Hand.ToArray(), Present.Done );
			if(myGift != null) {
				dst.Hand.Add( myGift );
				src.Hand.Remove( myGift );
			}
		}
	}
}

