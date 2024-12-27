namespace SpiritIsland.Horizons;

public class IntractableThicketsAndThorns {

	public const string Name = "Intractable Thickets and Thorns";

	[SpiritCard(Name, 2, Element.Moon, Element.Water,Element.Earth,Element.Plant), Fast, FromPresence(1)]
	[Instructions("1 Fear. Defend 5."), Artist(Artists.MoroRogers)]
	static public async Task ActAsync(TargetSpaceCtx ctx) {
		// 1 Fear.
		await ctx.AddFear(1);
		// Defend 5.
		ctx.Defend(5);
	}

}
