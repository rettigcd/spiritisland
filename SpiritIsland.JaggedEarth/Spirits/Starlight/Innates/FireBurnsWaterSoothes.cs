using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	[InnatePower("Fire Burns, Water Soothes"), Slow, FromSacredSite(1)]
	class FireBurnsWaterSoothes {

		[InnateOption("3 fire","1 Fear. 2 Damage.")]
		static public Task Option1(TargetSpaceCtx ctx ) {
			ctx.AddFear(1);
			return ctx.DamageInvaders(2);
		}

		[InnateOption("3 water","Remove 1 blight.",1)]
		static public Task Option2(TargetSpaceCtx ctx ) {
			return ctx.RemoveBlight();
		}

	}


}
