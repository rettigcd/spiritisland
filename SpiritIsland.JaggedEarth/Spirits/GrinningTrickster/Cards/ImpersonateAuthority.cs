namespace SpiritIsland.JaggedEarth;

public class ImpersonateAuthority {

	[SpiritCard("Impersonate Authority", 0, Element.Sun,Element.Air,Element.Animal), Slow, FromPresence(1)]
	static public Task ActAsymc(TargetSpaceCtx ctx ) {
		// Add 1 strife
		return ctx.AddStrife();
	}

}