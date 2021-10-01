using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class FlashFloods {

		public const string Name = "Flash Floods";
		[SpiritCard(FlashFloods.Name,2,Speed.Fast,Element.Sun,Element.Water)]
		[FromPresence(1,Target.Invaders)]
		static public async Task ActionAsync(TargetSpaceCtx ctx) {
			// +1 damage, if costal +1 additional damage
			int damage = ctx.Space.IsCoastal ? 2 : 1;
			await ctx.DamageInvaders( damage );
		}

	}

}