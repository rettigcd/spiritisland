namespace SpiritIsland.JaggedEarth;

[InnatePower("Why Don't You and Them Fight"), FastButSlowIf("3 moon"), FromPresence(0,Target.Invaders)]
class WhyDontYouAndThemFight {

	[DisplayOnly("3 moon","This Power may be slow.")]
	static public Task Option1(TargetSpaceCtx _ ) => Task.CompletedTask;

	[InnateOption("3 air","Add 1 strife.",2)]
	static public Task Option2(TargetSpaceCtx ctx ) => ctx.AddStrife();

	[InnateOption("3 sun","OR",3)]
	static public Task Option3a(TargetSpaceCtx ctx ) => Option3b(ctx);

	[InnateOption("3 fire","1 Invader and 1 dahan deal Damage to each other.",3)]
	static public async Task Option3b(TargetSpaceCtx ctx ) {
		var invaders = ctx.Tokens.InvaderTokens().ToArray();
		if(invaders.Length == 0 || !ctx.Dahan.Any) return;
		var invader = (HealthToken)await ctx.Decision(new Select.TokenFrom1Space("Select invader to fight 1 dahan",ctx.Space,invaders,Present.Always));

		// Calc Invader Damage
		var (damageFromInvader,newInvaderToken) = GetDamageFromInvader( ctx.Invaders, invader );
		// Calc Dahan Damage
		int damageFromDahan = 2;
			
		// Damage invader
		await ctx.Invaders.ApplyDamageTo1(damageFromDahan,newInvaderToken);

		// damage dahan
		if(damageFromInvader>=2)
			await ctx.DestroyDahan(1);

	}

	static (int,HealthToken) GetDamageFromInvader( InvaderBinding invaderBinding, HealthToken invader ) {
		return 0 < invader.StrifeCount
			? (0,invaderBinding.Tokens.RemoveStrife( invader, 1 )) 
			: (invaderBinding.Tokens.AttackDamageFrom1( invader ),invader);
	}

	[InnateOption("3 animal","If target land has beast, 2 Damage. Otherwise, you may Gather 1 beast.",4)]
	static public async Task Option4(TargetSpaceCtx ctx ) {
		// if target land has beasts
		if(ctx.Beasts.Any)
			// 2 damage
			await ctx.DamageInvaders(2);
		else // otherwise, you may gather 1 beast
			await ctx.GatherUpTo(1,TokenType.Beast);
	}


}