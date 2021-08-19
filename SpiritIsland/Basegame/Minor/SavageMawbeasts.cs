using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class SavageMawbeasts {

		[MinorCard("Savage Mawbeasts",0,Speed.Slow,Element.Fire,Element.Animal)]
		[FromSacredSite(1)]
		static public Task ActAsync(TargetSpaceCtx ctx){
			var target = ctx.Target;
			int damage = 0;

			// if target is J/W, 1 fear & 1 damage
			if(target.Terrain.IsIn( Terrain.Jungle, Terrain.Wetland )) {
				++damage;
				ctx.AddFear(1);
            }

			// If 3 animals +1 damage
			if(3 <= ctx.Self.Elements[Element.Animal] )
				++damage;

			return ctx.DamageInvaders( target, damage );
		}

	}

}
