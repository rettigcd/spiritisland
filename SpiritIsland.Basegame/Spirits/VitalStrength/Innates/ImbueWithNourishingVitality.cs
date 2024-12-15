namespace SpiritIsland.Basegame;

[InnatePower(Name), Fast, FromSacredSite(1,Filter.NoBlight)]
public class ImbueWithNourishingVitality {

	public const string Name = "Imbue with Nourishing Vitality";

	static public void InitAspect(Spirit spirit) {
		spirit.InnatePowers = [..spirit.InnatePowers,InnatePower.For(typeof(ImbueWithNourishingVitality))];
	}

	[InnateTier("1 water,1 plant", "Gather up to 1 Dahan.", 0)]
	static public Task Option1(TargetSpaceCtx ctx) {
		// Gather up to 1 Dahan.
		return ctx.GatherUpToNDahan(1);
	}

	[InnateTier("1 water,2 plant", "Add 1 Vitality.", 1)]
	static public Task Option2(TargetSpaceCtx ctx) {
		// Add 1 Vitality.
		return ctx.Space.AddAsync(Token.Vitality,1);
	}

	[InnateTier("1 water,1 earth,2 plant", "Each Dahan has +2 Health while in target land. If target land has at least 2 Dahan, Add 1 Dahan.", 2)]
	static public async Task Option3(TargetSpaceCtx ctx) {

		// Each Dahan has +2 Health while in target land.
		await ctx.Space.AdjustTokensHealthForRound(2, Human.Dahan);

		// If target land has at least 2 Dahan, Add 1 Dahan.
		if( 2 <= ctx.Dahan.CountAll )
			await ctx.Dahan.AddDefault(1);
	}

}