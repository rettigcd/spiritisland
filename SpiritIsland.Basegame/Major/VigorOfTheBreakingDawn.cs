﻿using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class VigorOfTheBreakingDawn {

		[MajorCard("Vigor of the Breaking Dawn",3,Element.Sun,Element.Animal)]
		[Fast]
		[FromPresence(2,Target.Dahan)]
		public static async Task ActAsync(TargetSpaceCtx ctx){

			// 2 damage per dahan in target land
			await ctx.DamageInvaders(2*ctx.DahanCount);

			if( ctx.YouHave("3 sun,2 animal") ){

				// you may push up to 2 dahan.
				var pushedToLands = await ctx.PushUpToNDahan(2);

				// 2 damage per dahan
				foreach(var neighbor in pushedToLands)
					await DahanDeal2DamageEach( ctx.Target( neighbor ) );

			}
		}

		static Task DahanDeal2DamageEach( TargetSpaceCtx ctx ) {
			return ctx.DamageInvaders( ctx.DahanCount * 2 );
		}
	}

}
