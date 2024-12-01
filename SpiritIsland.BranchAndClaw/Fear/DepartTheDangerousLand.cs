namespace SpiritIsland.BranchAndClaw;

class DepartTheDangerousLand : FearCardBase, IFearCard {

	public const string Name = "Depart the Dangerous Land";
	public string Text => Name;

	[FearLevel( 1, "Each player removes 1 Explorer from a land with Beast, Disease or at least 2 Dahan." )]
	public override Task Level1( GameState ctx ) 
		=> Cmd.RemoveExplorers( 1 )
			.From().SpiritPickedLand().Which( Has.BeastDiseaseOr2Dahan )
			.ForEachSpirit()
			.ActAsync( ctx );

	[FearLevel( 2, "Each player removes 1 Explorer/Town from a land with Beast, Disease or at least 2 Dahan." )]
	public override Task Level2( GameState ctx )
		=> Cmd.RemoveExplorersOrTowns(1)
			.From().SpiritPickedLand().Which( Has.BeastDiseaseOr2Dahan )
			.ForEachSpirit()
			.ActAsync( ctx );

	[FearLevel( 3, "Each player removes up to 4 health worth of Invaders from a land with Beast, Disease or at least 2 Dahan." )]
	public override Task Level3( GameState ctx )
		=> Cmd.RemoveUpToNHealthOfInvaders( 4 )
			.From().SpiritPickedLand().Which( Has.BeastDiseaseOr2Dahan )
			.ForEachSpirit()
			.ActAsync( ctx );

}