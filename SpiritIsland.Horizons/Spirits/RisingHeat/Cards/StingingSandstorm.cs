namespace SpiritIsland.Horizons;

public class StingingSandstorm {

	public const string Name = "Stinging Sandstorm";

	[SpiritCard(Name, 1, Element.Fire, Element.Air, Element.Earth), Slow, FromPresence(1)]
	[Instructions("Gather up to 1 of your Presence. 1 Fear and 1 Damage."), Artist(Artists.LucasDurham)]
	static public async Task ActAsync(TargetSpaceCtx ctx) {
		// Gather up to 1 of your Presence.
		await ctx.GatherUpTo(1,ctx.Self.Presence);
		// 1 Fear and 1 Damage.
		await ctx.AddFear(1);
		await ctx.DamageInvaders(1);
	}

}

