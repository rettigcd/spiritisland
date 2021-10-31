using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class UnrelentingGrowth {

		[MajorCard( "Unrelenting Growth", 4, Element.Sun, Element.Fire, Element.Water, Element.Plant )]
		[Slow]
		[AnySpirit]
		static public async Task ActAsync( TargetSpiritCtx ctx ) {

			// target spirit adds 2 presence and 1 wilds to a land at range 1

			// Select destination
			var to = await ctx.OtherCtx.SelectSpaceWithinRangeOfCurrentPresence( 1, Target.Any );

			// add wilds
			var toCtx = ctx.Target(to);
			toCtx.Wilds.Count++;

			// Add presence
			for(int i = 0; i < 2; ++i) {
				var from = await ctx.OtherCtx.SelectPresenceSource();
				await ctx.Other.Presence.PlaceFromBoard( from, to, ctx.GameState );
			}

			// if you have 3 sun, 3 plant
			if( await ctx.YouHave("3 sun,3 plant")) {
				// in that land add 1 additional wilds and remove 1 blight.  Target Spirit gains a power card.
				toCtx.Wilds.Count++;
				var blight = toCtx.Blight;
				if(blight.Any)
					blight.Count--;
			}

		}

	}

}
