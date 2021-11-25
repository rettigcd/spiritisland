using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	[InnatePower("Lost in the Swirling Haze"), Slow, FromPresence(0)]
	public class LostInTheSwirlingHaze {

		[InnateOption("1 air,2 water","Push up to 1 dahan.")]
		static public Task Option1(TargetSpaceCtx ctx) {
			return ctx.PushUpToNDahan(1);
		}

		[InnateOption("2 air,3 water","Push up to 2 explorer/dahan.")]
		static public Task Option2(TargetSpaceCtx ctx){
			return ctx.PushUpTo(2,Invader.Explorer,TokenType.Dahan);
		}

		[InnateOption("3 air,4 water","Push up to 2 explorer/dahan.")]
		static public Task Option3(TargetSpaceCtx ctx){
			return ctx.PushUpTo(2,Invader.Explorer,TokenType.Dahan);
		}

	}

}
