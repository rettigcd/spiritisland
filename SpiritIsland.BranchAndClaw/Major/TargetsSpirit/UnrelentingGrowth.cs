using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class UnrelentingGrowth {

		[MajorCard( "Unrelenting Growth", 4, Element.Sun, Element.Fire, Element.Water, Element.Plant )]
		[Slow]
		[AnySpirit]
		static public async Task ActAsync( TargetSpiritCtx ctx ) {

			TargetSpaceCtx toCtx = await AddPresenceAndWilds( ctx.OtherCtx );

			// if you have 3 sun, 3 plant
			if(await ctx.YouHave( "3 sun,3 plant" )) {
				// in that land add 1 additional wilds 
				toCtx.Wilds.Count++;
				// and remove 1 blight.
				var blight = toCtx.Blight;
				if(blight.Any) blight.Count--;
				// Target Spirit gains a power card.

			}

		}

		static async Task<TargetSpaceCtx> AddPresenceAndWilds( SpiritGameStateCtx ctx ) {

			// target spirit adds 2 presence and 1 wilds to a land at range 1

			// Select destination
			var to = await ctx.SelectSpaceWithinRangeOfCurrentPresence( 1, Target.Any ); // !!! this does not follow Lure / Volcano / Ocean special rules

			// add wilds
			var toCtx = ctx.Target( to );
			toCtx.Wilds.Count++;

			// Add presence
			for(int i = 0; i < 2; ++i) {
				var from = await ctx.Presence.SelectSource();
				await ctx.Self.Presence.PlaceFromTracks( from, to, ctx.GameState );
			}

			return toCtx;
		}
	}

}
