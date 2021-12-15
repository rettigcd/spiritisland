using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	class PourTimeSideways {

		[SpiritCard( "Pour Time Sideways", 1, Element.Moon, Element.Air, Element.Water ), Fast, Yourself]
		static public async Task ActAsync( SelfCtx ctx ) {
			if(ctx.Self is not FracturedDaysSplitTheSky frac) return;
			// Cost to Use: 3 Time
			if(frac.Time <3) return;
			frac.Time -= 3;

			// Move 1 of your presence to a different land with your presence.
			var src = await ctx.Self.Action.Decision( new Decision.Presence.Deployed( "Move presence from:", ctx.Self ) );
			var dstOptions = ctx.Self.Presence.Spaces.Where(s=>s!=src);
			var dst = await ctx.Self.Action.Decision( new Decision.AdjacentSpace( "Move preseence to:", src, Decision.AdjacentDirection.Outgoing, dstOptions ) );
			ctx.Presence.Move( src, dst );

			if(src.Board == dst.Board) return;

			// On the board moved from: During the Invader Phase, Resolve Invader and "Each board / Each land..." Actions one fewer time.
			foreach(var space in src.Board.Spaces) {
				ctx.GameState.Skip1Build(space);
				ctx.GameState.SkipExplore(space);
				ctx.GameState.SkipRavage(space);
			}

			// On the board moved to: During the Invader Phase, Resolve Invader and "Each board / Each Land..." Actions one more time.
			foreach(var space in src.Board.Spaces) {
				ctx.GameState.Add1Build( space );
				ctx.GameState.AddExplore( space );
				ctx.GameState.AddRavage( space );
			}
		}

	}

}
