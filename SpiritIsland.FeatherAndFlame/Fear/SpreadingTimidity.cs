
namespace SpiritIsland.FeatherAndFlame;

public class SpreadingTimidity : FearCardBase, IFearCard {
	public const string Name = "Spreading Timidity";
	public string Text => Name;

	[FearLevel( 1, "Each player chooses a land to Isolate." )]
	public override Task Level1( GameState ctx ) => Cmd.Isolate
		.In().SpiritPickedLand()
		.ForEachSpirit()
		.ActAsync( ctx );

	[FearLevel( 2, "Each player chooses a different land to Isolate. Also, Defend 2 in those lands." )]
	public override Task Level2( GameState ctx ) => new SpaceAction( $"Isolate target land and Defend 2.", ctx => { ctx.Isolate(); ctx.Defend( 2 ); } )
		.In().SpiritPickedLand().AllDifferent()
		.ForEachSpirit()
		.ActAsync( ctx );

	[FearLevel( 3, "Each player chooses a different land to Isolate. Also, Defend 4 in those lands." )]
	public override Task Level3( GameState ctx ) => new SpaceAction( $"Isolate target land and Defend 4.", ctx => { ctx.Isolate(); ctx.Defend( 4 ); } )
		.In().SpiritPickedLand().AllDifferent()
		.ForEachSpirit()
		.ActAsync( ctx );
}


