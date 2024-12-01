namespace SpiritIsland.Basegame;

public class TallTalesOfSavagery : FearCardBase, IFearCard {

	public const string Name = "Tall Tales of Savagery";
	public string Text => Name;

	[FearLevel( 1, "Each player removes 1 Explorer from a land with Dahan." )]
	public override Task Level1( GameState ctx )
		=> Cmd.RemoveExplorers( 1 )
			.From().SpiritPickedLand().Which( Has.DahanAndExplorers )
			.ForEachSpirit()
			.ActAsync( ctx );


	[FearLevel( 2, "Each player removes 2 Explorer or 1 Town from a land with Dahan." )]
	public override Task Level2( GameState ctx )
		=> Cmd.Pick1( Cmd.RemoveExplorers( 2 ), Cmd.RemoveTowns( 1 ) )
			.From().SpiritPickedLand().Which( Has.DahanAndExplorerOrTown )
			.ForEachSpirit()
			.ActAsync( ctx );

	[FearLevel( 3, "Remove 2 Explorer or 1 Town from each land with Dahan. Then, remove 1 City from each land with at least 2 Dahan." )]
	public override Task Level3( GameState ctx )
		=> Cmd.Multiple(
			RemoveTownOr2Explorers.On().EachActiveLand().Which( Has.DahanAndExplorerOrTown ),
			Cmd.RemoveCities( 1 ).On().EachActiveLand().Which( Has.Two2DahanAndCity )
		).ActAsync( ctx );

	static SpaceAction RemoveTownOr2Explorers => new SpaceAction( "Remove 2 Explorer or 1 Town", RemoveTownOr2Explorers_method );
	static async Task RemoveTownOr2Explorers_method( TargetSpaceCtx ctx ) { // !! maybe we should let the player choose in case town was strifed
		var invaders = ctx.Invaders;
		if(ctx.Space.Has( Human.Town ))
			await invaders.RemoveLeastDesirable( RemoveReason.Removed, Human.Town );
		else
			await invaders.RemoveLeastDesirable( RemoveReason.Removed, Human.Explorer );
			await invaders.RemoveLeastDesirable( RemoveReason.Removed, Human.Explorer );
	}

}