namespace SpiritIsland.BranchAndClaw;

public class PlanForDeparture : FearCardBase, IFearCard {

	public const string Name = "Plan for Departure";
	public string Text => Name;

	[FearLevel( 1, "Each player may gather 1 Town into a Coastal land." )]
	public override Task Level1( GameState ctx ) 
		=> Cmd.GatherUpToNInvaders( 1, Human.Town )
			.In().SpiritPickedLand().Which( Is.Coastal )
			.ForEachSpirit()
			.ActAsync( ctx );

	[FearLevel( 2, "Each player may gather 1 Explorer/Town into a Coastal land. Defend 2 in all Coastal lands." )]
	public override Task Level2( GameState ctx )
		=> Cmd.Multiple(
			Cmd.GatherUpToNInvaders( 1, Human.Explorer_Town ).In().SpiritPickedLand().Which( Is.Coastal ).ForEachSpirit(),
			Cmd.Defend( 2 ).In().EachActiveLand().Which( Is.Coastal )
		)
		.ActAsync( ctx );

	[FearLevel( 3, "Each player may gather 2 Explorer/Town into a Coastal land. Defend 4 in all Coastal lands." )]
	public override Task Level3( GameState ctx )
		=> Cmd.Multiple(
			Cmd.GatherUpToNInvaders( 2, Human.Explorer_Town ).In().SpiritPickedLand().Which( Is.Coastal ).ForEachSpirit(),
			Cmd.Defend( 4 ).In().EachActiveLand().Which( Is.Coastal )
		)
		.ActAsync( ctx );

}