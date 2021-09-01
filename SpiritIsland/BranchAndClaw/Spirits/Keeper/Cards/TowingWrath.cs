using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class TowingWrath {

		[SpiritCard("Towering Wrath",3,Speed.Slow,Element.Sun,Element.Fire,Element.Plant)]
		[FromSacredSite(1)]
		static public async Task ActAsync(TargetSpaceCtx ctx ) {

			// 2 fear
			ctx.AddFear( 2 );

			// for each of your SS in / adjacent to target land, 2 damage
			int sacredSiteCount = ctx.Target.Adjacent.Intersect( ctx.Self.SacredSites ).Count() + 1;
			await ctx.DamageInvaders( 2 * sacredSiteCount );

			// destroy all dahan
			await ctx.DestroyDahan(int.MaxValue,Cause.Power);
		}


	}

}
