using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	public class ImpersonateAuthority {
		[SpiritCard("Impersonate Authority", 0, Element.Sun,Element.Air,Element.Animal), Slow, FromPresence(1)]
		static public Task ActAsymc(TargetSpaceCtx _ ) {
			// Add 1 strife
			return Task.CompletedTask;
		}
	}
}
