namespace SpiritIsland.JaggedEarth;

[InnatePower("Why Don't You and Them Fight"), FastButSlowIf("3 moon"), FromPresence(0,Filter.Invaders)]
class WhyDontYouAndThemFight {

	[DisplayOnly("3 moon","This Power may be slow.")]
	static public Task Option1(TargetSpaceCtx _ ) => Task.CompletedTask;

	[InnateTier("3 air","Add 1 strife.",2)]
	static public Task Option2(TargetSpaceCtx ctx ) => ctx.AddStrife();

	[InnateTier("3 sun","OR",3)]
	static public Task Option3a(TargetSpaceCtx ctx ) => Option3b(ctx);

	[InnateTier("3 fire","1 Invader and 1 dahan deal Damage to each other.",3)]
	static public async Task Option3b(TargetSpaceCtx ctx ) {
		var invaders = ctx.Space.InvaderTokens().ToArray();
		if(invaders.Length == 0 || !ctx.Dahan.Any) return;
		var decision = new A.SpaceTokenDecision( "Select invader to fight 1 dahan", invaders.OnScopeTokens1(ctx.SpaceSpec), Present.Always );
		var spaceInvader = (await ctx.SelectAsync(decision))?.Token.AsHuman();

		// Calc Invader Damage
		var (damageFromInvader,newInvaderToken) = await GetDamageFromInvader( ctx.Invaders, spaceInvader );
		// Calc Dahan Damage
		int damageFromDahan = 2;
			
		// Damage invader
		await ctx.Invaders.ApplyDamageTo1(damageFromDahan,newInvaderToken);

		// damage dahan
		if(damageFromInvader>=2)
			await ctx.Dahan.Destroy(1);

	}

	static async Task<(int,HumanToken)> GetDamageFromInvader( InvaderBinding invaderBinding, HumanToken invader ) {
		return 0 < invader.StrifeCount
			? (0,await invaderBinding.Space.Remove1StrifeFromAsync( invader, 1 )) 
			: (invader.Attack, invader);
	}

	[InnateTier("3 animal","If target land has beast, 2 Damage. Otherwise, you may Gather 1 beast.",4)]
	static public async Task Option4(TargetSpaceCtx ctx ) {
		// if target land has beasts
		if(ctx.Beasts.Any)
			// 2 damage
			await ctx.DamageInvaders(2);
		else // otherwise, you may gather 1 beast
			await ctx.GatherUpTo(1,Token.Beast);
	}


}