using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class EntwinedPower {


		[MajorCard( "Entwined Power", 2, Speed.Fast, Element.Moon, Element.Water, Element.Plant )]
		[TargetSpirit]
		static public async Task ActAsync( TargetSpiritCtx ctx ) {
			var self = ctx.Self;
			var gs = ctx.GameState;
			var target = ctx.Target;

			// !!! You and target spirit may use each other's presence to target powers. - IMPLEMENT

			TargetLandApi x = self.PowerApi;

			// Target spirit gains a power Card.
			await ctx.Target.Draw( gs,(cards)=>{
				// You gain one of the power Cards they did not keep.
				return DrawFromDeck.TakeCard(self,cards);
			} );

			// if you have 2 water, 4 plant, 
			if(self.Elements.Contains("2 water,4 plant" )) {
				// you and target spirit each gain 3 energy
				self.Energy += 3;
				ctx.Target.Energy += 3;
				// may gift the other 1 power from hand.
				await GiftCardToSpirit( self, target );
				await GiftCardToSpirit( target, self );

			}
		}

		private static async Task GiftCardToSpirit( Spirit src, Spirit dst ) {
			var myGift = (PowerCard)await src.SelectFactory( "Select gift for " + dst.Text, src.Hand.ToArray(), Present.Done );
			if(myGift != null) {
				dst.Hand.Add( myGift );
				src.Hand.Remove( myGift );
			}
		}
	}
}

