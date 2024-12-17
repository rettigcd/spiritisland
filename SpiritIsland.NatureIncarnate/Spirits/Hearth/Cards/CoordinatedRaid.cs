namespace SpiritIsland.NatureIncarnate;

public class CoordinatedRaid {

	public const string Name = "Coordinated Raid";

	[SpiritCard(Name, 1, Element.Sun,Element.Earth,Element.Animal)]
	[Slow, FromPresence( Filter.Dahan, 1, Filter.Any )]
	[Instructions( "1 Damage. If Dahan are present, 1 Damage." ), Artist( Artists.AalaaYassin )]
	static public async Task ActAsync(TargetSpaceCtx ctx) {

		// 1 Damage. If Dahan are present, 1 Damage.
		await ctx.DamageInvaders( ctx.Dahan.Any ? 2 : 1 );

	}

}