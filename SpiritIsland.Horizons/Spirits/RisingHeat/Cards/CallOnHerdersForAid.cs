namespace SpiritIsland.Horizons;

public class CallOnHerdersForAid {

	public const string Name = "Call on Herders for Aid";

	[SpiritCard(Name,1,Element.Sun,Element.Fire,Element.Earth,Element.Animal),Slow,FromPresence(2)]
	[Instructions( "Gather up to 2 Dahan. For each Dahan present, Push up to 1 Explorer/Town/Dahan." ), Artist( Artists.LucasDurham )]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {
		// Gather up to 2 Dahan.
		await ctx.GatherUpToNDahan(2);
		// For each Dahan present, Push up to 1 Explorer/Town/Dahan.
		await ctx.PushUpTo(ctx.Dahan.CountAll, Human.Dahan, Human.Explorer, Human.Town );
	}

}

