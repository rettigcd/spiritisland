using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class EntwinedPower {


		[MajorCard( "Entwined Power", 2, Speed.Fast, Element.Moon, Element.Water, Element.Plant )]
		[TargetSpirit]
		static public async Task ActAsync( ActionEngine selfEng, Spirit target ) {
			var (self, gs) = selfEng;
			// !!! You and target spirit may use each other's presence to target powers.

			TargetLandApi x = self.PowerCardApi;

			// Target spirit gains a power Card.
			var targetEng = target.Bind( selfEng.GameState);
			await selfEng.Self.CardDrawer.Draw(targetEng,(cards)=>{
				// You gain one of the power Cards they did not keep.
				return DrawFromDeck.TakeCard(selfEng,cards);
			} );

			// if you have 2 water, 4 plant, 
			if(self.Elements.Contains("2 water,4 plant" )) {
				// you and target spirit each gain 3 energy
				self.Energy += 3;
				target.Energy += 3;
				// may gift the other 1 power from hand.
				await GiftCardToSpirit( selfEng, target );
				await GiftCardToSpirit( targetEng, self );

			}
		}

		private static async Task GiftCardToSpirit( ActionEngine srcEngine, Spirit dst ) {
			var src = srcEngine.Self;
			var myGift = (PowerCard)await srcEngine.Self.SelectFactory( "Select gift for " + dst.Text, src.Hand.ToArray(), true );
			if(myGift != null) {
				dst.Hand.Add( myGift );
				src.Hand.Remove( myGift );
			}
		}
	}
}

