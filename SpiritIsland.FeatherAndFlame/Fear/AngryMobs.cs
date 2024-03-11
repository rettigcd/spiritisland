namespace SpiritIsland.FeatherAndFlame;

public class AngryMobs : FearCardBase, IFearCard {

	public const string Name = "Angry Mobs";
	public string Text => Name;

	[FearLevel( "Each player may replace 1 Town with 2 Explorer. 1 Fear per player who does." )]
	public Task Level1( GameState ctx )
		=> new SpaceAction( "may replace 1 Town with 2 Explorer and gain 1 Fear.", Level1_MayReplace1TownWith2ExplorersAndGain1Fear )
			.In().SpiritPickedLand()
			.ByPickingToken( Human.Town )
			.ForEachSpirit()
			.ActAsync( ctx );

	[FearLevel( "In each land with 2 or more Explorer, destroy 1 Explorer/Town per 2 Explorer." )] 
	public Task Level2( GameState ctx )
		=> Level2_Each2ExplorersDestroy_ExplorerOrTown
			.In().EachActiveLand() // Don't need filter because it is on the Command
			.ActAsync( ctx );

	[FearLevel( "In each land with 2 or more Explorer, destroy 1 Invader per 2 Explorer." )] 
	public Task Level3( GameState GameState )
		=> Level3_Each2ExplorersDestroy_Invader
			.In().EachActiveLand() // don't need filter because it is on the command.
			.ActAsync( GameState );

	static async Task Level1_MayReplace1TownWith2ExplorersAndGain1Fear( TargetSpaceCtx ctx ) {
		var options = ctx.Space.HumanOfTag( Human.Town ).OnScopeTokens1( ctx.SpaceSpec );
		var st = await ctx.Self.SelectAsync( new A.SpaceTokenDecision( "Replace 1 Town with 2 Explorers", options, Present.Done ) );
		if(st == null) return;
		HumanToken town = st.Token.AsHuman();

		int explorersToAdd = Math.Min(town.RemainingHealth,2); // don't let Durable towns create 4
		await ctx.Space.ReplaceAsync( town.AsHuman(), explorersToAdd, ctx.Space.GetDefault( Human.Explorer ) );

		ctx.AddFear( 1 );
	}


	static public SpaceAction Level2_Each2ExplorersDestroy_ExplorerOrTown => new SpaceAction(
		$"Destroy 1 Explorer/Towns per 2 Explorers",
		ctx => ctx.Invaders.DestroyNOfAnyClass( ctx.Space.Sum( Human.Explorer ) / 2, Human.Explorer_Town )
	).OnlyExecuteIf( Has2OrMoreExplorers );

	static public SpaceAction Level3_Each2ExplorersDestroy_Invader => new SpaceAction(
		$"Destroy 1 Invader per 2 Explorers",
		ctx => ctx.Invaders.DestroyNOfAnyClass( ctx.Space.Sum( Human.Explorer ) / 2, Human.Invader )
	).OnlyExecuteIf( Has2OrMoreExplorers );

	static bool Has2OrMoreExplorers( TargetSpaceCtx ss ) => 2 <= ss.Space.Sum( Human.Explorer );

}
