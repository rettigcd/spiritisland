using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {
	public class RazorSharpUndergrowth {

		[MinorCard( "Razor-Sharp Undergrowth", 1, Speed.Fast, Element.Moon, Element.Plant )]
		[FromPresence( 0, Target.NoBlight )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			await ctx.InvadersOn(ctx.Target).Destroy(1,Invader.Explorer);

			await ctx.GameState.DahanDestroy(ctx.Target,1,Cause.Power);

			ctx.Tokens[BacTokens.Wilds]++;

			ctx.Defend(2);

		}

	}
}
