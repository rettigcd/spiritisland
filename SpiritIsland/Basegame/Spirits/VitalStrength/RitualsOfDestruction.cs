using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame.Spirits.VitalStrength {
	
	class RitualsOfDestruction {

		[SpiritCard("Rituals of Destruction",3,Speed.Slow,Element.Sun,Element.Moon,Element.Fire,Element.Earth,Element.Plant)]
		[FromSacredSite(1,Target.Dahan)]
		static public Task ActAsync(TargetSpaceCtx ctx){

			bool hasBonus = 3 <= ctx.GameState.GetDahanOnSpace(ctx.Target);

			if(hasBonus)
				ctx.AddFear( 2 );

			return ctx.DamageInvaders(ctx.Target, hasBonus ? 5 : 2);

		}

	}

}
