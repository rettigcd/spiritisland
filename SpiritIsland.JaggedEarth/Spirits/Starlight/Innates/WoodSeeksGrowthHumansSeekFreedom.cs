namespace SpiritIsland.JaggedEarth;

[InnatePower("Wood Seeks Growth, Humans Seek Freedom"), Slow, FromPresence(2)]
class WoodSeeksGrowthHumansSeekFreedom {

	[InnateOption("3 plant","Choose a Spirit with presence in target land. They gain a Power Card.")]
	static public async Task Option1( TargetSpaceCtx ctx ) {
		var spiritOptions = ctx.GameState.Spirits.Where( s=> s.Presence.IsOn( ctx.Tokens ) ).ToArray();
		if(spiritOptions.Length > 0) return;
		var spirit = await ctx.Decision(new Select.ASpirit("Select spirit to gain a power card", spiritOptions));

		await ctx.TargetSpirit( spirit ).OtherCtx.Draw();
	}

	[InnateOption("3 animal","1 Damage per dahan OR Push up to 3 dahan.",1)]
	static public Task Option2(TargetSpaceCtx ctx ) {
		return ctx.SelectActionOption( 
			DahanDamage, 
			Cmd.PushUpToNDahan(3)
		);
	}

	static SpaceAction DahanDamage => new SpaceAction("1 damage / dahan", ctx => ctx.DamageInvaders(ctx.Dahan.CountAll) );

}