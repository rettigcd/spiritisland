namespace SpiritIsland.NatureIncarnate;

public class FocusTheSunsRays {
	const string Name = "Focus the Sun's Rays";
	[SpiritCard(Name,0,Element.Sun,Element.Fire,Element.Air),Slow,FromSacredSite(2)]
	[Instructions( "1 Damage. 2 Damage to Dahan. Move up to 3 Presence directly to target land (from anywhere). You may Bring 1 Badlands (total) with those Presence." ), Artist( Artists.AgnieszkaDabrowiecka )]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {
		// 1 Damage.
		await ctx.DamageInvaders(1);
		// 2 Damage to Dahan.
		await ctx.DamageDahan(2);
		// Move up to 3 Presence directly to target land (from anywhere).
		SpaceToken[] sources = await new TokenCollectorFromSpecifiedSources( ctx, ctx.Self.Presence.Spaces.Tokens().ToArray() )
			.AddGroup( 3, ctx.Self.Presence.Token ) // !! this won't gather Incarna if spirit gets incarna
			.CollectUpToN();

		// You may Bring 1 Badlands (total) with those Presence.
		SpaceState[] sourcesWithBadlands = sources
			.Select(x=>x.Space.Tokens)
			.Where(tokens=>tokens.Badlands.Any )
			.ToArray();
		await new TokenCollectorFromSpecifiedSources( ctx, sourcesWithBadlands )
			.AddGroup( 1, Token.Badlands )
			.CollectUpToN();

	}

}