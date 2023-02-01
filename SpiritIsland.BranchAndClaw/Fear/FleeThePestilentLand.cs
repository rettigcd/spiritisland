namespace SpiritIsland.BranchAndClaw;

class FleeThePestilentLand : FearCardBase, IFearCard {

	public const string Name = "Flee the Pestilent Land";
	public string Text => Name;

	[FearLevel( 1, "Each player removes 1 Explorer/Town from a land with Disease." )]
	public Task Level1( GameCtx ctx )
		=> Cmd.RemoveExplorersOrTowns(1)
			.In().SpiritPickedLand().Which( Has.Disease )
			.ByPickingToken(Human.Explorer_Town)
			.ForEachSpirit()
			.Execute( ctx );

	[FearLevel( 2, "Each player removes up to 3 health of Invaders from a land with Disease or 1 Explorer from an inland land." )]
	public Task Level2( GameCtx ctx ) // !! Flatten by selecting first space, then action is to remove it and optionally 2 more
		=> Cmd.Pick1<SelfCtx>(
			Cmd.RemoveUpToNHealthOfInvaders( 3 ).From().SpiritPickedLand().Which( Has.Disease ),
			Cmd.RemoveExplorers( 1 ).From().SpiritPickedLand().Which( Is.Inland )
		).ForEachSpirit()
		.Execute(ctx);


	[FearLevel( 3, "Each player removes up to 5 health of Invaders from a land with Disease or 1 Explorer/Town from an inland land." )]
	public Task Level3( GameCtx ctx ) // !! Flatten by selecting first space, then action is to remove it and optionally 2 more
		=> Cmd.Pick1<SelfCtx>(
			Cmd.RemoveUpToNHealthOfInvaders( 5 ).From().SpiritPickedLand().Which( Has.Disease ),
			Cmd.RemoveExplorersOrTowns( 1 ).From().SpiritPickedLand().Which( Is.Inland )
		).ForEachSpirit()
		.Execute(ctx);

}