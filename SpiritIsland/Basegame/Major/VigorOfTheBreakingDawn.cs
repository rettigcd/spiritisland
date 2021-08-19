using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class VigorOfTheBreakingDawn {

		[MajorCard("Vigor of the Breaking Dawn",3,Speed.Fast,Element.Sun,Element.Animal)]
		[FromPresence(2,Target.Dahan)]
		public static async Task ActAsync(TargetSpaceCtx ctx){
			var (spirit,gs) = ctx;

			// 2 damage per dahan in target land
			await ctx.DamageInvaders(ctx.Target,2*gs.GetDahanOnSpace(ctx.Target));

			if( spirit.Elements.Contains("3 sun,2 animal") ){

				// you may push up to 2 dahan.
				var pushedToLands = await ctx.PushUpToNDahan(ctx.Target, 2);

				// 2 damage per dahan
				foreach(var neighbor in pushedToLands )
					await ctx.DamageInvaders( neighbor, 2*gs.GetDahanOnSpace(neighbor) );
			}
		}

	}

}
