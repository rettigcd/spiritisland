namespace SpiritIsland.Horizons;

public class OpenShiftingWaterways {

	public const string Name = "Open Shifting Waterways";

	[SpiritCard(Name,1,Element.Moon,Element.Water,Element.Animal),Slow,FromPresence(1)]
	[Instructions( "Gather up to 2 Dahan. If Dahan and Invaders are present, 1 Fear and 1 Damage." ), Artist( Artists.MoroRogers)]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {
		// Gather up to 2 Dahan.
		await ctx.GatherUpToNDahan(2);
		// If Dahan and Invaders are present, 1 Fear and 1 Damage.
		if(ctx.HasInvaders && ctx.Dahan.Any ) {
			await ctx.AddFear(1);
			await ctx.DamageInvaders(1);
		}
	}

}
