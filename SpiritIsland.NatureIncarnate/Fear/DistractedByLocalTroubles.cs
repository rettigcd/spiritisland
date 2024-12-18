namespace SpiritIsland.NatureIncarnate;

public partial class DistractedByLocalTroubles : FearCardBase, IFearCard {

	public const string Name = "Distracted by Local Troubles";
	public string Text => Name;

	[FearLevel(1, "On Each Board, in a land matching a Ravage Card: 1 Damage." )]
	public override Task Level1( GameState gs )
		=> Cmd.DamageInvaders(1)
			.In().OneLandPerBoard().Which( Is.RavageCardMatch )
			.ForEachBoard()
			.ActAsync( gs );

	[FearLevel( 2, "Invaders do -1 Damage per Damage they have taken. On Each Board, in a land matching a Ravage Card: 1 Damage each to up to 2 Invaders." )]
	public override Task Level2( GameState gs )
		=> Cmd.Multiple(
			ReduceInvaderAttackByDamage,
			OneDamageEachToUpTo2Invaders
			.In().OneLandPerBoard().Which( Is.RavageCardMatch )
			.ForEachBoard()
		).ActAsync( gs );

	[FearLevel( 3, "Invaders do -1 Damage per Damage they have taken. On Each Board, in two lands matching a Ravage Card: 2 Damage (per land)." )]
	public override Task Level3( GameState gs )	
		=> Cmd.Multiple(
			ReduceInvaderAttackByDamage,
			Cmd.DamageInvaders(2)
				.In().NDifferentLandsPerBoard(2).Which( Is.RavageCardMatch )
				.ForEachBoard()
			).ActAsync( gs );

	static SpaceAction OneDamageEachToUpTo2Invaders => new SpaceAction("1 Damage each to up to 2 Invaders", async ctx=>{ 
		var invadersToDamage = ctx.SourceSelector
			.AddAll(Human.Invader)
			.ConfigOnlySelectEachOnce()
			.GetEnumerator(ctx.Self,Prompt.RemainingCount("Damage-1 each"), Present.Done, null, 2);
		await foreach(var invader in invadersToDamage)
			await ctx.Invaders.ApplyDamageTo1(1,invader.Token.AsHuman());

	});

	static BaseCmd<GameState> ReduceInvaderAttackByDamage => new BaseCmd<GameState>(
		"Invaders do -1 Attack / Damage", 
		gs=>gs.Tokens.AddIslandMod(new AdjustDamageFromAttackers( ReduceAttackByReceivedDamage ) )
	);

	static int ReduceAttackByReceivedDamage( RavageExchange ravageExchange )
		=> - ravageExchange.Attackers.Active.Sum(pair => Math.Min(pair.Key.Attack,pair.Key.Damage) * pair.Value );


}

