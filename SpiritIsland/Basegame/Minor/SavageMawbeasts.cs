using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class SavageMawbeasts {

		[MinorCard("Savage Mawbeasts",0,Speed.Slow,Element.Fire,Element.Animal)]
		[FromSacredSite(1)]
		static public Task ActAsync(ActionEngine engine,Space target){
			int damage = 0;

			// if target is J/W, 1 fear & 1 damage
			if(target.Terrain.IsIn( Terrain.Jungle, Terrain.Wetland )) {
				++damage;
				engine.GameState.AddFear(1);
            }

			// If 3 animals +1 damage
			if(3 <= engine.Self.Elements[Element.Animal] )
				++damage;

			engine.GameState.DamageInvaders( target, damage );

			return Task.CompletedTask;
		}

	}

}
