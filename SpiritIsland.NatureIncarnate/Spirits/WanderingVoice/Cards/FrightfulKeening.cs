namespace SpiritIsland.NatureIncarnate;

public class FrightfulKeening {

	public const string Name = "Frightful Keening";

	[SpiritCard(Name, 1, Element.Sun, Element.Fire, Element.Air)]
	[Slow, FromIncarna]
	[Instructions( "Push Incarna. If this pushes Incarna into a land with Invaders, 2 Fear there (before adding Strife)." ), Artist( Artists.EmilyHancock )]
	static public async Task ActionAsync(TargetSpaceCtx ctx){
		if( ctx.Self.Presence is not IHaveIncarna ihi || ihi.Incarna.Space == null) return;

		// Push Incarna.
		await new PushIncarna().ActAsync(ctx);

		// If this pushes Incarna into a land with Invaders,
		var dst = ihi.Incarna.Space;
		if( dst.HasInvaders() )
			// 2 Fear there (before adding Strife).
			ctx.Target(dst).AddFear(2);
	}

}