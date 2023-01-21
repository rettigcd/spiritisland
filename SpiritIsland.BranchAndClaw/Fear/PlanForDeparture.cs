namespace SpiritIsland.BranchAndClaw;

public class PlanForDeparture : FearCardBase, IFearCard {

	public const string Name = "Plan for Departure";
	public string Text => Name;

	[FearLevel( 1, "Each player may gather 1 town into a costal land." )]
	public Task Level1( GameCtx ctx ) 
		=> Cmd.GatherUpToNInvaders( 1, Invader.Town )
			.In().SpiritPickedLand().Which( Is.Coastal )
			.ForEachSpirit()
			.Execute( ctx );

	[FearLevel( 2, "Each player may gather 1 explroer / town into a costal land.  Defend 2 in all costal lands." )]
	public Task Level2( GameCtx ctx )
		=> Cmd.Multiple(
			Cmd.GatherUpToNInvaders( 1, Invader.Explorer_Town ).In().SpiritPickedLand().Which( Is.Coastal ).ForEachSpirit(),
			Cmd.Defend( 2 ).In().EachActiveLand().Which( Is.Coastal )
        )
		.Execute( ctx );

	[FearLevel( 3, "Each player may gather 2 explorer / town into a costal land.  Defend 4 in all costal lands" )]
	public Task Level3( GameCtx ctx )
		=> Cmd.Multiple(
			Cmd.GatherUpToNInvaders( 2, Invader.Explorer_Town ).In().SpiritPickedLand().Which( Is.Coastal ).ForEachSpirit(),
			Cmd.Defend( 4 ).In().EachActiveLand().Which( Is.Coastal )
		)
		.Execute( ctx );

}