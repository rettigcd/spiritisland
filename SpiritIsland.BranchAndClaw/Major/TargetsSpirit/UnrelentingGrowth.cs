using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class UnrelentingGrowth {

		[MajorCard( "Unrelenting Growth", 4, Speed.Slow, Element.Sun, Element.Fire, Element.Water, Element.Plant )]
		[TargetSpirit]
		static public async Task ActAsync( TargetSpiritCtx ctx ) {

			// target spirit adds 2 presence and 1 wilds to a land at range 1
			Space[] destinationOptions = ctx.TargetCtx.Presence_DestinationOptions( 1, Target.Any );
			var to = await ctx.Target.Action.Decide( new TargetSpaceDecision( "Where would you like to place your presence?", destinationOptions, Present.Always ) );
			var tokens = ctx.GameState.Tokens[to];
			var wilds = tokens.Wilds();
			wilds.Count++;
			for(int i = 0; i < 2; ++i) {
				var from = await ctx.Target.SelectTrack();
				await ctx.Target.Presence.PlaceFromBoard( from, to, ctx.GameState );
			}

			// if you have 3 sun, 3 plant
			if( ctx.Self.Elements.Contains("3 sun,3 plant")) {
				// in that land add 1 additional wilds and remove 1 blight.  Target Spirit gains a power card.
				wilds.Count++;
				var blight = tokens.Blight;
				if(blight.Any)
					blight.Count--;
			}

		}

	}

}
