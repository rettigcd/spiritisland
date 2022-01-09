using System;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {

	[InnatePower(DarknessSwallowsTheUnwary.Name),Fast]
	[FromSacredSite(1)]
	public class DarknessSwallowsTheUnwary {

		public const string Name = "Darkness Swallows the Unwary";

		[InnateOption("2 moon,1 fire","Gather 1 explorer.")]
		public static async Task Gather1Explorer( TargetSpaceCtx ctx ) {
			await ctx.GatherUpTo( 1, Invader.Explorer );
		}

		[InnateOption("3 moon,2 fire","Destroy up to 2 explorer. 1 fear per explorer destroyed.")]
		public static async Task Plus_Destroy2Explorers( TargetSpaceCtx ctx ){
			await Gather1Explorer(ctx);

			// destroy 2 explorers (+1 fear/kill)
			var grp = ctx.Invaders;
			int destroyed = await grp.Destroy( 2, Invader.Explorer );
			ctx.AddFear( destroyed );
		}

		[InnateOption("4 moon,3 fire,2 air","3 Damage. 1 fear per Invaders destroyed by this Damage.")]
		public static async Task Plus_3Damage( TargetSpaceCtx ctx ){
			await Plus_Destroy2Explorers(ctx);

			// 3 more points of damage (+ 1 fear/kill )
			int startingCount = ctx.Invaders.Tokens.InvaderTotal();
			await ctx.DamageInvaders( 3 );
			int endingCount = ctx.Invaders.Tokens.InvaderTotal();
			int killed = startingCount - endingCount;
			ctx.AddFear( killed );
		}

	}

}
