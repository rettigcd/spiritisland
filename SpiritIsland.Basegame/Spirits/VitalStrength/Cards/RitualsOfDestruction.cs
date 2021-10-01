using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame.Spirits.VitalStrength {
	
	class RitualsOfDestruction {

		[SpiritCard("Rituals of Destruction",3,Speed.Slow,Element.Sun,Element.Moon,Element.Fire,Element.Earth,Element.Plant)]
		[FromSacredSite(1,Target.Dahan)]
		static public Task ActAsync(TargetSpaceCtx ctx){

			// 2 damage
			int damage = 2;

			if( 3 <= ctx.DahanCount ) {
				damage += 3;
				ctx.AddFear( 2 );
			}

			return ctx.DamageInvaders( damage );

		}

	}

}
