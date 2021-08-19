using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	class CleansingFloods { 

		[MajorCard("Cleansing Floods",5, Speed.Slow, Element.Sun, Element.Water)]
		[FromPresenceIn(1,Terrain.Wetland)]
		static public Task ActAsync(TargetSpaceCtx ctx) {
			// 4 damage, remove 1 blight
			// if you have 4 water, +10 damage
			ctx.RemoveBlight();
			int damage = (4 <= ctx.Self.Elements[Element.Water])
				? 14
				: 4;
			return ctx.DamageInvaders(ctx.Target,damage);
		}

	}

}
