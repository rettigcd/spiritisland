namespace SpiritIsland.Horizons;

[InnatePower(Name,"-description-")]
[Slow,FromPresence(1,Filter.Invaders)]
public class DeathApproachesFromBeneathTheSurface {

	public const string Name = "Death Approaches from Beneath the Surface";

	[InnateTier("1 fire,1 animal", "If you don't have Presence in target land, Gather 1 of your Presence. (This is required.)", 0)]
	static public Task Option1(TargetSpaceCtx ctx) {
		return ctx.Gather(1, ctx.Self.Presence);
	}

	[InnateTier("2 fire,1 earth,2 animal", "1 Damage", 1)]
	static public Task Option2( TargetSpaceCtx ctx ) {
		return ctx.DamageInvaders(1);
	}

	[InnateTier("3 fire,1 earth,3 animal", "2 Damage", 1)]
	static public Task Option3(TargetSpaceCtx ctx) {
		return ctx.DamageInvaders(1+2);
	}

	[InnateTier("4 fire,2 earth,5 animal", "2 Fear. 4 Damage", 1)]
	static public async Task Option4(TargetSpaceCtx ctx) {
		await ctx.AddFear(2);
		await ctx.DamageInvaders(1 + 2 + 4);
	}

}
