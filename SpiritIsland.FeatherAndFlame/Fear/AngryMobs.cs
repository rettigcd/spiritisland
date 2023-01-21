
namespace SpiritIsland.FeatherAndFlame;

public class AngryMobs : FearCardBase, IFearCard {

	public const string Name = "Angry Mobs";
	public string Text => Name;

	[FearLevel( 1, "Each player may replace 1 Town with 2 Explorer. 1 Fear per player who does." )]
	public Task Level1( GameCtx ctx )
		=> new SpaceAction( "may replace 1 Town with 2 Explorer and gain 1 Fear.", Level1_MayReplace1TownWith2ExplorersAndGain1Fear )
			.In().SpiritPickedLand()
			.ByPickingToken( Invader.Town )
			.ForEachSpirit()
			.Execute( ctx );

	[FearLevel( 2, "In each land with 2 or more Explorer, destroy 1 Explorer / Town per 2 Explorer." )] 
	public Task Level2( GameCtx ctx )
		=> Level2_Each2ExplorersDestroy_ExplorerOrTown
			.In().EachActiveLand() // Don't need filter because it is on the Command
			.Execute( ctx );

	[FearLevel( 3, "Terror Level 3: In each land with 2 or more Explorer, destroy 1 Invader per 2 Explorer." )] 
	public Task Level3( GameCtx gameCtx )
		=> Level3_Each2ExplorersDestroy_Invader
			.In().EachActiveLand() // don't need filter because it is on the command.
			.Execute( gameCtx );

	static async Task Level1_MayReplace1TownWith2ExplorersAndGain1Fear( TargetSpaceCtx ctx ) {
		var token = (await ctx.Self.Gateway.Decision( new Select.TokenFrom1Space( "Replace 1 Town with 2 Explorers", ctx.Space, ctx.Tokens.OfClass(Invader.Town).Cast<IVisibleToken>(), Present.Done ) ))?.Token;
		if(token == null) return;

		ctx.Tokens.Adjust( token, -1 );
		ctx.Tokens.AdjustDefault( Invader.Explorer, 2 );

		ctx.AddFear( 1 );
	}


	static public SpaceAction Level2_Each2ExplorersDestroy_ExplorerOrTown => new SpaceAction(
		$"Destroy 1 Explorer/Towns per 2 Explorers",
		ctx => ctx.Invaders.DestroyNOfAnyClass( ctx.Tokens.Sum( Invader.Explorer ) / 2, Invader.Explorer_Town )
	).OnlyExecuteIf( Has2OrMoreExplorers );

	static public SpaceAction Level3_Each2ExplorersDestroy_Invader => new SpaceAction(
		$"Destroy 1 Invader per 2 Explorers",
		ctx => ctx.Invaders.DestroyNOfAnyClass( ctx.Tokens.Sum( Invader.Explorer ) / 2, Invader.Any )
	).OnlyExecuteIf( Has2OrMoreExplorers );

	static bool Has2OrMoreExplorers( TargetSpaceCtx ss ) => 2 <= ss.Tokens.Sum( Invader.Explorer );

}
