using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	public class WallsOfRockAndThorn {

		[MajorCard("Walls of Rock and Thorn",4,Element.Sun,Element.Earth,Element.Plant), Fast, FromSacredSite(2,Target.JungleOrMountain)]
		public static async Task ActAsync(TargetSpaceCtx ctx ) {
			// 2 damage.
			await ctx.DamageInvaders(2);

			// defend 8.
			ctx.Defend(8);

			// add 1 wilds.
			await ctx.Wilds.Add(1);

			// isolate target land
			ctx.Isolate();

			// if you have 2 earth, 2 plant:
			if( await ctx.YouHave("2 earth,2 plant" )) {
				// +2 damage.
				await ctx.DamageInvaders(2);
				// +2 defend.
				ctx.Defend(2);
				// Add 1 badland.
				await ctx.Badlands.Add(1);
			}

		}

	}


}
