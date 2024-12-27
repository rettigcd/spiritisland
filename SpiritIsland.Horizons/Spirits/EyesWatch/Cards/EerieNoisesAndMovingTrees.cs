namespace SpiritIsland.Horizons;

public class EerieNoisesAndMovingTrees {

	public const string Name = "Eerie Noises and Moving Trees";

	[SpiritCard(Name, 2, Element.Moon, Element.Air,Element.Plant), Slow, FromPresence(1)]
	[Instructions("2 Fear. Push up to 2 Explorer/Town."), Artist(Artists.MoroRogers)]
	static public async Task ActAsync(TargetSpaceCtx ctx) {
		// 2 Fear.
		await ctx.AddFear(2);
		// Push up to 2 Explorer/Town.
		await ctx.PushUpTo(2,Human.Explorer_Town);
	}

}
