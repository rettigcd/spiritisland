using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class EntwinedPower {


		[MajorCard( "Entwined Power", 2, Speed.Fast, Element.Moon, Element.Water, Element.Plant )]
		[TargetSpirit]
		static public async Task ActAsync( TargetSpiritCtx ctx ) {
			var gs = ctx.GameState;

			ConfigSharedPresenceForTargeting( ctx );

			// Target spirit gains a power Card.
			await ctx.Target.Draw( gs, ( cards ) => {
				// You gain one of the power Cards they did not keep.
				return DrawFromDeck.TakeCard( ctx.Self, cards );
			} );

			// if you have 2 water, 4 plant, 
			if(ctx.Self.Elements.Contains( "2 water,4 plant" )) {
				// you and target spirit each gain 3 energy
				ctx.Self.Energy += 3;
				ctx.Target.Energy += 3;
				// may gift the other 1 power from hand.
				await GiftCardToSpirit( ctx.Self, ctx.Target );
				await GiftCardToSpirit( ctx.Target, ctx.Self );

			}
		}

		static void ConfigSharedPresenceForTargeting( TargetSpiritCtx ctx ) {
			// You and target spirit may use each other's presence to target powers. - IMPLEMENT
			// - capture old so we can restore it
			var oldSelfApi = ctx.Self.PowerApi;
			var oldTargetApi = ctx.Target.PowerApi;
			Task Restore( GameState _ ) {
				ctx.Self.PowerApi = oldSelfApi;
				ctx.Target.PowerApi = oldTargetApi;
				return Task.CompletedTask;
			}
			// - update to shared
			var sharedPowerApi = new SharedPresenceTargeting( ctx.Self, ctx.Target );
			ctx.Self.PowerApi = sharedPowerApi;
			ctx.Target.PowerApi = sharedPowerApi;
			// put it back when we are done
			ctx.GameState.TimePasses_ThisRound.Push( Restore );
		}

		static async Task GiftCardToSpirit( Spirit src, Spirit dst ) {
			var myGift = (PowerCard)await src.Select( "Select gift for " + dst.Text, src.Hand.ToArray(), Present.Done );
			if(myGift != null) {
				dst.Hand.Add( myGift );
				src.Hand.Remove( myGift );
			}
		}
	}

	class SharedPresenceTargeting : TargetLandApi {

		readonly Spirit[] spirits;

		public SharedPresenceTargeting( params Spirit[] spirits ) {
			this.spirits = spirits;
		}

		protected override IEnumerable<Space> GetTargetOptions( Spirit _, From sourceEnum, Terrain? sourceTerrain, int range, string filterEnum, GameState gameState ) {
			return spirits
				.SelectMany( spirit => base.GetTargetOptions( spirit, sourceEnum, sourceTerrain, range, filterEnum, gameState ) )
				.Distinct();
		}

	}

}

