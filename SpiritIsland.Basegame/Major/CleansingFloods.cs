using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	public class CleansingFloods { 

		[MajorCard("Cleansing Floods",5, Element.Sun, Element.Water)]
		[Slow]
		[FromPresenceIn(1,Terrain.Wetland)]
		static public async Task ActAsync(TargetSpaceCtx ctx) {

			// 4 damage
			int damage = 4;

			// remove 1 blight
			await ctx.RemoveBlight();

			// if you have 4 water, +10 damage
			if(await ctx .YouHave("4 water"))
				damage += 10;

			await ctx.DamageInvaders(damage);
		}

	}

}
