namespace SpiritIsland.FeatherAndFlame;

public class AngryMobs : FearCardBase, IFearCard {

	public const string Name = "Angry Mobs";
	public string Text => Name;

	[FearLevel( 1, "Each player may replace 1 Town with 2 Explorer. 1 Fear per player who does." )]
	public Task Level1( GameCtx ctx )
		=> new SpaceAction( "may replace 1 Town with 2 Explorer and gain 1 Fear.", Level1_MayReplace1TownWith2ExplorersAndGain1Fear )
			.In().SpiritPickedLand()
			.ByPickingToken( Human.Town )
			.ForEachSpirit()
			.Execute( ctx );

	[FearLevel( 2, "In each land with 2 or more Explorer, destroy 1 Explorer/Town per 2 Explorer." )] 
	public Task Level2( GameCtx ctx )
		=> Level2_Each2ExplorersDestroy_ExplorerOrTown
			.In().EachActiveLand() // Don't need filter because it is on the Command
			.Execute( ctx );

	[FearLevel( 3, "In each land with 2 or more Explorer, destroy 1 Invader per 2 Explorer." )] 
	public Task Level3( GameCtx gameCtx )
		=> Level3_Each2ExplorersDestroy_Invader
			.In().EachActiveLand() // don't need filter because it is on the command.
			.Execute( gameCtx );

	static async Task Level1_MayReplace1TownWith2ExplorersAndGain1Fear( TargetSpaceCtx ctx ) {
		var options = ctx.Tokens.OfHumanClass( Human.Town );
		var token = (await ctx.Self.Gateway.Decision( new Select.TokenFromManySpaces( "Replace 1 Town with 2 Explorers", ctx.Space, options, Present.Done ) ))?.Token;
		if(token == null) return;

		ctx.Tokens.Adjust( token, -1 );
		ctx.Tokens.AdjustDefault( Human.Explorer, 2 );

		ctx.AddFear( 1 );
	}


	static public SpaceAction Level2_Each2ExplorersDestroy_ExplorerOrTown => new SpaceAction(
		$"Destroy 1 Explorer/Towns per 2 Explorers",
		ctx => ctx.Invaders.DestroyNOfAnyClass( ctx.Tokens.Sum( Human.Explorer ) / 2, Human.Explorer_Town )
	).OnlyExecuteIf( Has2OrMoreExplorers );

	static public SpaceAction Level3_Each2ExplorersDestroy_Invader => new SpaceAction(
		$"Destroy 1 Invader per 2 Explorers",
		ctx => ctx.Invaders.DestroyNOfAnyClass( ctx.Tokens.Sum( Human.Explorer ) / 2, Human.Invader )
	).OnlyExecuteIf( Has2OrMoreExplorers );

	static bool Has2OrMoreExplorers( TargetSpaceCtx ss ) => 2 <= ss.Tokens.Sum( Human.Explorer );

}
