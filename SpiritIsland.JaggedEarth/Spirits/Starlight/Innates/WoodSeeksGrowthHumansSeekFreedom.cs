namespace SpiritIsland.JaggedEarth;

[InnatePower("Wood Seeks Growth, Humans Seek Freedom"), Slow, FromPresence(2)]
class WoodSeeksGrowthHumansSeekFreedom {

	[InnateTier("3 plant","Choose a Spirit with presence in target land. They gain a Power Card.")]
	static public async Task Option1( TargetSpaceCtx ctx ) {
		var spiritOptions = GameState.Current.Spirits.Where( s=> s.Presence.IsOn( ctx.Tokens ) ).ToArray();
		if(spiritOptions.Length > 0) return;
		var spirit = await ctx.SelectAsync(new A.Spirit("Select spirit to gain a power card", spiritOptions));

		await ctx.Self.Target( spirit ).Other.Draw();
	}

	[InnateTier("3 animal","1 Damage per dahan OR Push up to 3 dahan.",1)]
	static public Task Option2(TargetSpaceCtx ctx ) {
		return ctx.SelectActionOption( 
			DahanDamage, 
			Cmd.PushUpToNDahan(3)
		);
	}

	static SpaceCmd DahanDamage => new SpaceCmd("1 damage / dahan", ctx => ctx.DamageInvaders(ctx.Dahan.CountAll) );

}