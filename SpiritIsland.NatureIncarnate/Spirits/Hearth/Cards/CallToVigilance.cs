namespace SpiritIsland.NatureIncarnate;

public class CallToVigilance {

	public const string Name = "Call to Vigilance";

	[SpiritCard(Name, 2, Element.Sun, Element.Air, Element.Animal)]
	[Slow, FromPresence(Filter.Dahan,1,Filter.Any)]
	[Instructions( "2 Fear if Invaders are present. For each Dahan in origin land, Push up to 1 Explorer/Town." ), Artist( Artists.AalaaYassin )]
	static public async Task ActionAsync(TargetSpaceCtx ctx){

		// 2 Fear if Invaders are present.
		if(ctx.HasInvaders)
			ctx.AddFear(2);

		// For each Dahan in origin land, Push up to 1 Explorer/Town.
		int pushCount = ctx.Self
			.FindTargettingSourcesFor(
				ctx.SpaceSpec,
				new TargetingSourceCriteria( TargetFrom.Presence ),
				new TargetCriteria( 1 )
			)
			.Max(s=>s.Dahan.CountAll);
		await ctx.PushUpTo(pushCount,Human.Explorer_Town);

	}

}