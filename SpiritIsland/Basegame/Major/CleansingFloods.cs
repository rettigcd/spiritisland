using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	class CleansingFloods { 

		[MajorCard("Cleansing Floods",5, Speed.Slow, Element.Sun, Element.Water)]
		[FromPresenceIn(1,Terrain.Wetland)]
		static public Task ActAsync(ActionEngine engine, Space target ) {
			// 4 damage, remove 1 blight
			// if you have 4 water, +10 damage
			engine.GameState.RemoveBlight(target);
			int damage = (4 <= engine.Self.Elements[Element.Water])
				? 14
				: 4;
			return engine.DamageInvaders(target,damage);
		}

	}

}
