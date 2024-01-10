namespace SpiritIsland.NatureIncarnate;

[InnatePower( Name )]
[Fast, FromPresence( 1 )]
public class AfflictWithBloodThirst {

	public const string Name = "Afflict with Bloodthirst";

	[InnateTier( "1 animal", "Gather 1 Beast.", 0 )]
	static public Task Option1( TargetSpaceCtx ctx ) => ctx.Gather(1, Token.Beast);

	[InnateTier( "1 fire,3 animal", "2 Fear if Invaders are present.", 1 )]
	static public Task Option2( TargetSpaceCtx ctx ) {
		if(ctx.HasInvaders)
			ctx.AddFear(2);
		return Task.CompletedTask;
	}

	[InnateTier( "1 sun,2 fire,4 animal", "1 Explorer and 1 Town/Dahan do Damage, to other Invaders only.", 2 )]
	static public async Task Option3( TargetSpaceCtx ctx ) {
		// Init Invaders
		var invaders = new CountDictionary<HumanToken>();
		foreach(var human in ctx.Tokens.HumanOfAnyTag( TokenCategory.Invader ))
			invaders[human] = ctx.Tokens[human];
		// Init Attackers
		var attackers = new CountDictionary<HumanToken>();
		void MakeAttacker(HumanToken? token) {
			if(token is null) return;
			attackers[token]++;
			if(0<invaders[token]) invaders[token]--;
		}
		MakeAttacker( ctx.Tokens.HumanOfTag( Human.Explorer ).FirstOrDefault() );
		MakeAttacker( ctx.Tokens.HumanOfTag( Human.Dahan ).FirstOrDefault()
			?? ctx.Tokens.HumanOfTag( Human.Town ).FirstOrDefault()
		);
		await new RavageExchange(
			ctx.Tokens,
			RavageOrder.DahanTurn, // not sure matters
			new RavageParticipants( attackers, attackers ),
			new RavageParticipants( invaders, new CountDictionary<HumanToken>() ) // invaders do not participate
		).Execute( RavageBehavior.DefaultBehavior );
	}

	[InnateTier( "1 sun,2 animal", "For each Beast, Push 1 Explorer and 1 Town/Dahan.", 3 )]
	static public async Task Option4( TargetSpaceCtx ctx ) {
		int beastCount = ctx.Beasts.Count;
		await ctx.SourceSelector
			.AddGroup(beastCount,Human.Explorer)
			.AddGroup(beastCount,Human.Town,Human.Dahan)
			.PushN(ctx.Self);
	}

}
