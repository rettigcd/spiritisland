using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	public class UnexpectedTigers {
		[SpiritCard("Unexpected Tigers",0,Element.Moon,Element.Fire,Element.Animal), Slow, FromPresence(0) ]
		static public Task ActAsymc(TargetSpaceCtx _ ) { 
			// 1 fear if unvaders are present.
			// If you can gather 1 beast, do so, then push 1 explorer.
			// othersie, add 1 beast
			return Task.CompletedTask;
		}
	}
}
