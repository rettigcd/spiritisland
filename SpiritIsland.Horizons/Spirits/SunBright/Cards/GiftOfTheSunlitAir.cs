namespace SpiritIsland.Horizons;

public class GiftOfTheSunlitAir {

	public const string Name = "Gift of the Sunlit Air";

	[SpiritCard(Name, 0, Element.Sun, Element.Fire, Element.Air), Fast, AnySpirit]
	[Instructions("Target Spirit gets +1 range with all their Powers.  If you target another Spirit, they gain 1 Energy."), Artist(Artists.LucasDurham)]
	static public Task ActAsync(TargetSpiritCtx ctx) {
		// Target Spirit gets +1 range with all their Powers.
		RangeExtender.Extend(ctx.Other, 1);

		// If you target another Spirit, they gain 1 Energy.
		if( ctx.Other != ctx.Self )
			ctx.Other.Energy++;
		return Task.CompletedTask;
	}

}
