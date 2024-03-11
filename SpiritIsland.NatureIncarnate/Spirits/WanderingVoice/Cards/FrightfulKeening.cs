namespace SpiritIsland.NatureIncarnate;

public class FrightfulKeening {

	public const string Name = "Frightful Keening";

	[SpiritCard(Name, 1, Element.Sun, Element.Fire, Element.Air)]
	[Slow, FromIncarna]
	[Instructions( "Push Incarna. If this pushes Incarna into a land with Invaders, 2 Fear there (before adding Strife)." ), Artist( Artists.EmilyHancock )]
	static public async Task ActionAsync(TargetSpaceCtx ctx){
		var incarna = ctx.Self.Incarna;
		if( !incarna.IsPlaced ) return;

		// Push Incarna.
		await new PushIncarna().ActAsync(ctx.Self);

		// If this pushes Incarna into a land with Invaders,
		var dst = incarna.Space; // pushed to new space
		if( dst.HasInvaders() )
			// 2 Fear there
			// (before adding Strife). - why does this matter???
			ctx.Target(dst).AddFear(2);
	}

}