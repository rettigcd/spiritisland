using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {
	public class RazorSharpUndergrowth {

		[MinorCard( "Razor-Sharp Undergrowth", 1, Speed.Fast, Element.Moon, Element.Plant )]
		[FromPresence( 0, Target.NoBlight )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			// destory 1 explorer
			await ctx.Invaders.Destroy(1,Invader.Explorer);
			// and 1 dahan
			await ctx.GameState.DahanDestroy(ctx.Space,1,Cause.Power); // !!! can this be foldeded int TargetSpaceContext to remove some of the parameters
			// add 1 wilds
			ctx.Tokens.Wilds().Count++;
			// defend 2
			ctx.Defend(2);

		}

	}
}
