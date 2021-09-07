using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class VigorOfTheBreakingDawn {

		[MajorCard("Vigor of the Breaking Dawn",3,Speed.Fast,Element.Sun,Element.Animal)]
		[FromPresence(2,Target.Dahan)]
		public static async Task ActAsync(TargetSpaceCtx ctx){

			// 2 damage per dahan in target land
			await ctx.DamageInvaders(2*ctx.DahanCount);

			if( ctx.Self.Elements.Contains("3 sun,2 animal") ){

				// you may push up to 2 dahan.
				var pushedToLands = await ctx.PushUpToNTokens(2);

				// 2 damage per dahan
				foreach(var neighbor in pushedToLands )
					await ctx.DamageInvaders( neighbor, 2*ctx.GameState.DahanGetCount(neighbor) );
			}
		}

	}

}
