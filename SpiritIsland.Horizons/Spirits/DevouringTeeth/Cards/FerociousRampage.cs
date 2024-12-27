namespace SpiritIsland.Horizons;

public class FerociousRampage {

	public const string Name = "Ferocious Rampage";

	[SpiritCard(Name, 2, Element.Fire, Element.Animal), Slow, FromPresence(0)]
	[Instructions("1 Fear. 3 Damage to Explorer/Town only."), Artist(Artists.MoroRogers)]
	static public async Task ActAsync(TargetSpaceCtx ctx) {
		// 1 Fear.
		await ctx.AddFear(1);
		// 3 Damage to Explorer/Town only.
		await ctx.DamageInvaders(3,Human.Explorer_Town);
	}

}
