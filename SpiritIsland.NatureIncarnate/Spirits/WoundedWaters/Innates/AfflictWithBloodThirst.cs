namespace SpiritIsland.NatureIncarnate;

[InnatePower( Name )]
[Fast, FromPresence( 1 )]
public class AfflictWithBloodThirst {

	public const string Name = "Afflict with Bloodthirst";

	[InnateOption( "1 animal", "Gather 1 Beast.", 0 )]
	static public Task Option1( TargetSpaceCtx ctx ) => ctx.Gather(1, Token.Beast);

	[InnateOption( "1 fire,3 animal", "2 Fear if Invaders are present.", 1 )]
	static public Task Option2( TargetSpaceCtx ctx ) {
		if(ctx.HasInvaders)
			ctx.AddFear(2);
		return Task.CompletedTask;
	}

	[InnateOption( "1 sun,2 fire,4 animal", "1 Explorer and 1 Town/Dahan do Damage, to other Invaders only.", 2 )]
	static public async Task Option3( TargetSpaceCtx ctx ) {
		// Collect Town & explorer that is going to do damage
		// (and temporarily remove them)
		int damage = 0;
		var doingDamage = new List<ISpaceEntity>();
		var explorer = ctx.Tokens.Keys.FirstOrDefault(x=>x.Class == Human.Explorer);
		var town = ctx.Tokens.Keys.FirstOrDefault( x => x.Class == Human.Town );
		if(explorer is not null) {
			ctx.Tokens.Adjust(explorer,-1);
			doingDamage.Add(explorer);
			damage++;
		}
		if(ctx.Dahan.Any) {
			damage += 2;
		} else if(town is not null) {
			doingDamage.Add(town);
			damage += 2;
			ctx.Tokens.Adjust(town,-1);
		}

		// Do Damage
		await ctx.DamageInvaders(damage);
		// Restore them
		foreach(var t in doingDamage)
			ctx.Tokens.Adjust(t,1);
	}

	[InnateOption( "1 sun,2 animal", "For each Beast, Push 1 Explorer and 1 Town/Dahan.", 3 )]
	static public async Task Option4( TargetSpaceCtx ctx ) {
		int beastCount = ctx.Beasts.Count;
		await ctx.Pusher
			.AddGroup(beastCount,Human.Explorer)
			.AddGroup(beastCount,Human.Town,Human.Dahan)
			.MoveN();
	}

}
