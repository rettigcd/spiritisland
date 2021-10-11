using System.Threading.Tasks;

namespace SpiritIsland.PromoPack1 {
	public class FlashFires {

		[SpiritCard("Flash-Fires",2,Element.Fire,Element.Air)]
		[SlowButFastIf("2 air")]
		[FromSacredSite(2)] 
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			// 1 fear
			ctx.AddFear(1);

			// 1 damage
			await ctx.DamageInvaders(1);

			// if you have 2 air, this power is fast

		}

	}



}
