using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class UnrelentingGrowth {

		[MajorCard( "Unrelenting Growth", 4, Speed.Slow, Element.Sun, Element.Fire, Element.Water, Element.Plant )]
		[TargetSpirit]
		static public async Task ActAsync( TargetSpiritCtx ctx ) {

			// target spirit adds 2 presence and 1 wilds to a land at range 1

			// Select destination
			Space[] destinationOptions = ctx.OtherCtx.Presence_DestinationOptions( 1, Target.Any );
			var to = await ctx.Other.Action.Decision( new Decision.TargetSpace( "Where would you like to place your presence?", destinationOptions, Present.Always ) );

			// add wilds
			var tokens = ctx.GameState.Tokens[to];
			var wilds = tokens.Wilds();
			wilds.Count++;

			// Add presence
			for(int i = 0; i < 2; ++i) {
				var from = await ctx.Other.SelectTrack();
				await ctx.Other.Presence.PlaceFromBoard( from, to, ctx.GameState );
			}

			// if you have 3 sun, 3 plant
			if( ctx.YouHave( "3 sun,3 plant")) {
				// in that land add 1 additional wilds and remove 1 blight.  Target Spirit gains a power card.
				wilds.Count++;
				var blight = tokens.Blight;
				if(blight.Any)
					blight.Count--;
			}

		}

	}

}
