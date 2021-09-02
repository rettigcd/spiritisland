using System;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {

	[InnatePower(DarknessSwallowsTheUnwary.Name,Speed.Fast)]
	[FromSacredSite(1)]
	public class DarknessSwallowsTheUnwary {

		public const string Name = "Darkness Swallows the Unwary";

		[InnateOption("2 moon, 1 fire")]
		public static async Task Gather1Explorer( TargetSpaceCtx ctx ) {
			await ctx.GatherUpToNTokens( ctx.Target, 1, Invader.Explorer );
		}

		[InnateOption("3 moon, 2 fire")]
		public static async Task Plus_Destory2Explorers( TargetSpaceCtx ctx ){
			await Gather1Explorer(ctx);

			// destory 2 explorers (+1 fear/kill)
			var grp = ctx.PowerInvaders;
			int destroyed = await grp.Destroy(Invader.Explorer, 2 );
			ctx.AddFear( destroyed );
		}

		[InnateOption("3 moon, 2 fire")]
		public static async Task Plus_3Damage( TargetSpaceCtx ctx ){
			await Plus_Destory2Explorers(ctx);

			// 3 more points of damage (+ 1 fear/kill )
			int startingCount = ctx.PowerInvaders.Counts.InvaderTotal();
			await ctx.DamageInvaders(ctx.Target, 3 );
			int endingCount = ctx.PowerInvaders.Counts.InvaderTotal();
			int killed = startingCount - endingCount;
			ctx.AddFear( killed );
		}

	}

}
