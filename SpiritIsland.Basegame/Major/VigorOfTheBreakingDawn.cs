using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class VigorOfTheBreakingDawn {

		[MajorCard("Vigor of the Breaking Dawn",3,Speed.Fast,Element.Sun,Element.Animal)]
		[FromPresence(2,Target.Dahan)]
		public static async Task ActAsync(TargetSpaceCtx ctx){

			// 2 damage per dahan in target land
			await ctx.DamageInvaders(2*ctx.DahanCount);

			if( ctx.YouHave("3 sun,2 animal") ){

				// you may push up to 2 dahan.
				var pushedToLands = await ctx.PushUpToNDahan(2);

				// 2 damage per dahan
				foreach(var neighbor in pushedToLands)
					DahanDeal2DamageEach( ctx.TargetSpace( neighbor ) );

			}
		}

		static void DahanDeal2DamageEach( TargetSpaceCtx ctx ) {
			ctx.DamageInvaders( ctx.DahanCount * 2 );
		}
	}

}
