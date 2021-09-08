using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class FireAndFlood {

		[MajorCard( "Fire and Flood", 9, Speed.Slow, Element.Sun, Element.Fire, Element.Water )]
		[FromSacredSite( 1 )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			// == Pick 2nd target - range 2 from same SS ==
			var spiritSS = ctx.Self.SacredSites.ToArray();
			var ss = ctx.Target.Range(1).Where(s=>spiritSS.Contains(s)).ToArray();
			var secondOptions = ctx.Self.PowerApi.GetTargetOptions( ctx.Self,ctx.GameState,ss,2,Target.Any);
			var secondTarget = await ctx.Self.Action.Decide( new TargetSpaceDecision( "Select space to target.", secondOptions ) );

			// 4 damage in each target land  (range must be measured from same SS)
			await ctx.DamageInvaders( 4 );
			await ctx.InvadersOn(secondTarget).ApplySmartDamageToGroup(4);
			
		}

	}

}
