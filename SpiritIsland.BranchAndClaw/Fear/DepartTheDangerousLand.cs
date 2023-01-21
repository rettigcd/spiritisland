namespace SpiritIsland.BranchAndClaw;

class DepartTheDangerousLand : FearCardBase, IFearCard {

	public const string Name = "Depart the Dangerous Land";
	public string Text => Name;

	[FearLevel( 1, "Each player removes 1 explorer from a land with beast, disease or at least 2 dahan" )]
	public Task Level1( GameCtx ctx ) 
		=> Cmd.RemoveExplorers( 1 )
			.From().SpiritPickedLand().Which( Has.BeastDiseaseOr2Dahan )
			.ForEachSpirit()
			.Execute( ctx );

	[FearLevel( 2, "Each player removes 1 explorer/town from a land with beast, disease or at least 2 dahan" )]
	public Task Level2( GameCtx ctx )
		=> Cmd.RemoveExplorersOrTowns(1)
			.From().SpiritPickedLand().Which( Has.BeastDiseaseOr2Dahan )
			.ForEachSpirit()
			.Execute( ctx );


	[FearLevel( 3, "Each player removes up to 4 health worth of invaders from a land with beast, disease or at least 2 dahan" )]
	public Task Level3( GameCtx ctx )
		=> Cmd.RemoveUpToNHealthOfInvaders( 4 )
			.From().SpiritPickedLand().Which( Has.BeastDiseaseOr2Dahan )
			.ForEachSpirit()
			.Execute( ctx );


}