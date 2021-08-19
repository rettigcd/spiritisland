using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	class CleansingFloods { 

		[MajorCard("Cleansing Floods",5, Speed.Slow, Element.Sun, Element.Water)]
		[FromPresenceIn(1,Terrain.Wetland)]
		static public Task ActAsync(TargetSpaceCtx ctx) {
			// remove 1 blight
			ctx.RemoveBlight();
			// 4 damage
			int damage = 4
				// if you have 4 water, +10 damage
				+ (4 <= ctx.Self.Elements[Element.Water] ? 10 : 0);
			return ctx.DamageInvaders(damage);
		}

	}

}
