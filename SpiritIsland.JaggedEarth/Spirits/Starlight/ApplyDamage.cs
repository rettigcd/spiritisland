namespace SpiritIsland.JaggedEarth;

class ApplyDamage : GrowthActionFactory {

	public override async Task ActivateAsync( SelfCtx ctx ) {
		var space = await ctx.Decision(new Select.ASpace("Select land to apply 2 Damage.", ctx.Self.Presence.Spaces, Present.Always));
		await ctx.Target(space).DamageInvaders(2);
	}

}
