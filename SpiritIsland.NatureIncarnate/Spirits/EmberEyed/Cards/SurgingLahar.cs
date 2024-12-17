namespace SpiritIsland.NatureIncarnate;

public class SurgingLahar {

	public const string Name = "Surging Lahar";

	[SpiritCard(Name, 2, Element.Fire, Element.Water,Element.Earth)]
	[Slow, FromSacredSite(1)]
	[Instructions( "2 Damage. If your Presence is present, Add 1 Badlands" ), Artist( Artists.DavidMarkiwsky )]
	static public async Task ActAsync(TargetSpaceCtx ctx){
		// 2 Damage.
		await ctx.DamageInvaders(2);
		// If your Presence is present,
		if(ctx.IsPresent)
			// Add 1 Badland
			await ctx.Badlands.AddAsync(1);
	}

}