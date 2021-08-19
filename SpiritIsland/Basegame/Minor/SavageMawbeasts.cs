﻿using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class SavageMawbeasts {

		[MinorCard("Savage Mawbeasts",0,Speed.Slow,Element.Fire,Element.Animal)]
		[FromSacredSite(1)]
		static public Task ActAsync(TargetSpaceCtx ctx){
			int damage = 0;

			// if target is J/W, 1 fear & 1 damage
			if(ctx.IsOneOf( Terrain.Jungle, Terrain.Wetland )) {
				++damage;
				ctx.AddFear(1);
            }

			// If 3 animals +1 damage
			if( ctx.Self.Elements.Contains("3 animal") )
				++damage;

			return ctx.DamageInvaders( damage );
		}

	}

}
