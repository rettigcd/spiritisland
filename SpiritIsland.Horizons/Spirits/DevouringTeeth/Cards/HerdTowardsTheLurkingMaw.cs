namespace SpiritIsland.Horizons;

public class HerdTowardsTheLurkingMaw {

	public const string Name = "Herd Towards the Lurking Maw";

	[SpiritCard(Name, 1, Element.Water, Element.Earth, Element.Animal), Slow, FromPresence(0)]
	[Instructions("1 Fear. Gather up to 1 Explorer/Town."), Artist(Artists.MoroRogers)]
	static public async Task ActAsync(TargetSpaceCtx ctx) {
		await ctx.AddFear(1);
		await ctx.GatherUpTo(1, Human.Explorer_Town);
	}

}
