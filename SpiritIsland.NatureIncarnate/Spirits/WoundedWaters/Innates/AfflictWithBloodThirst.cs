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
		// Collect Town & explorer that is going to do damage
		// (and temporarily remove them)
		int damage = 0;
		List<HumanToken> doingDamage = new List<HumanToken>();

		// Add Explorer
		HumanToken? explorer = ctx.Tokens.HumanOfTag(Human.Explorer).FirstOrDefault(); // !!! what if it is strifed?
		if(explorer is not null) {
			ctx.Tokens.Adjust(explorer,-1);
			doingDamage.Add(explorer);
			damage++;
		}
		// Dahan or Town
		if(ctx.Dahan.Any)
			damage += 2;
			// don't need to add to because DamageInvaders doesn't effect dahan
		else {
			var town = ctx.Tokens.HumanOfTag(Human.Town).FirstOrDefault();
			if(town is not null) {
				doingDamage.Add(town);
				damage += 2;
				ctx.Tokens.Adjust(town,-1);
			}
		}

		// Do Damage
		await ctx.DamageInvaders(damage);
		// Restore them
		foreach(HumanToken t in doingDamage)
			ctx.Tokens.Adjust(t,1);
	}

	[InnateTier( "1 sun,2 animal", "For each Beast, Push 1 Explorer and 1 Town/Dahan.", 3 )]
	static public async Task Option4( TargetSpaceCtx ctx ) {
		int beastCount = ctx.Beasts.Count;
		await ctx.Pusher
			.AddGroup(beastCount,Human.Explorer)
			.AddGroup(beastCount,Human.Town,Human.Dahan)
			.DoN();
	}

}
