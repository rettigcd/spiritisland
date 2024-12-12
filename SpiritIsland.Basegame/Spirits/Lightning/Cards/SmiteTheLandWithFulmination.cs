namespace SpiritIsland.Basegame;

public class SmiteTheLandWithFulmination {
	public const string Name = "Smite the Land with Fulmination";

	[SpiritCard(Name, 2, Element.Sun, Element.Fire, Element.Air), Slow, FromPresence(1)]
	[Instructions("1 Damage. Add 1 Badlands."), Artist(Artists.DavidMarkiwsky)]
	static public async Task ActAsync(TargetSpaceCtx ctx) {
		await ctx.DamageInvaders(1);
		await ctx.Badlands.AddAsync(1);
	}

}