using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class FireAndFlood {

		[MajorCard( "Fire and Flood", 9, Speed.Slow, Element.Sun, Element.Fire, Element.Water )]
		[FromSacredSite( 1 )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			// == Pick 2nd target - range 2 from same SS ==
			var spiritSS = ctx.Self.SacredSites.ToArray();
			var possibleSacredSiteSourcesForThisSpace = ctx.Space.Range(1).Where(s=>spiritSS.Contains(s)).ToArray();
			var secondTargetOptions = ctx.FindSpacesWithinRangeOf( possibleSacredSiteSourcesForThisSpace, 2, Target.Any );

			var secondTarget = await ctx.Self.Action.Decision( new Decision.TargetSpace( "Select space to target.", secondTargetOptions, Present.Always ) );


			// 4 damage in each target land  (range must be measured from same SS)
			await ctx.DamageInvaders( 4 );
			await ctx.Target(secondTarget).DamageInvaders(4);
			
			// if 3 fire
			if(ctx.YouHave("3 fire" ))
				await Apply3DamageInOneOfThese( ctx, secondTarget, "fire" );

			// if 3 water
			if(ctx.YouHave( "3 water" ))
				await Apply3DamageInOneOfThese( ctx, secondTarget, "water" );

		}

		static async Task Apply3DamageInOneOfThese( TargetSpaceCtx ctx, Space secondTarget, string damageType ) {
			var space = await ctx.Self.Action.Decision( new Decision.TargetSpace( "Apply 3 " + damageType + " damage to", new Space[] { ctx.Space, secondTarget }, Present.Always ) );
			await ctx.Target( space ).DamageInvaders( 3 );
		}
	}

}
