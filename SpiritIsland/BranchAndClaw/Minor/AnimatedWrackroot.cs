using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw.Minor {

	public class AnimatedWrackroot {

		[MinorCard( "Animated WrackRoot", 0, Speed.Slow, Element.Moon, Element.Fire, Element.Plant )]
		[FromPresence( 0 )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			if(await ctx.Self.UserSelectsFirstText( "Select power", "1 fear, Destory 1 explorer", "Add 1 wilds" )) {
				ctx.AddFear(1);
				await ctx.InvadersOn(ctx.Target).Destroy(1,Invader.Explorer[1]);
			} else {
				ctx.Tokens[BacTokens.Wilds]++;
			}
		}

	}


}
