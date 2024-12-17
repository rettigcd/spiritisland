namespace SpiritIsland.JaggedEarth;

public class ImpersonateAuthority {

	[SpiritCard("Impersonate Authority", 0, Element.Sun,Element.Air,Element.Animal), Slow, FromPresence(1)]
	[Instructions( "Add 1 Strife." ), Artist( Artists.JoshuaWright )]
	static public Task ActAsync(TargetSpaceCtx ctx ) {
		// Add 1 strife
		return ctx.AddStrife();
	}

}