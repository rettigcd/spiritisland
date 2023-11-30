namespace SpiritIsland.JaggedEarth;

class ApplyDamage : SpiritAction {

	public ApplyDamage():base( "ApplyDamage" ) { }

	public override async Task ActAsync( SelfCtx ctx ) {
		var space = await ctx.SelectAsync(new A.Space("Select land to apply 2 Damage.", ctx.Self.Presence.Lands, Present.Always));
		await ctx.Target(space).DamageInvaders(2);
	}

}
