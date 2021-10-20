using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame.Spirits.VitalStrength {
	
	class RitualsOfDestruction {

		[SpiritCard("Rituals of Destruction",3,Element.Sun,Element.Moon,Element.Fire,Element.Earth,Element.Plant)]
		[Slow]
		[FromSacredSite(1,Target.Dahan)]
		static public Task ActAsync(TargetSpaceCtx ctx){

			// 2 damage
			int damage = 2;

			if( 3 <= ctx.Dahan.Count ) {
				damage += 3;
				ctx.AddFear( 2 );
			}

			return ctx.DamageInvaders( damage );

		}

	}

}
