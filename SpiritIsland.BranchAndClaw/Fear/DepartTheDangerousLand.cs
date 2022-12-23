namespace SpiritIsland.BranchAndClaw;

class DepartTheDangerousLand : IFearCard {

	public const string Name = "Depart the Dangerous Land";
	public string Text => Name;
	public int? Activation { get; set; }
	public bool Flipped { get; set; }

	[FearLevel( 1, "Each player removes 1 explorer from a land with beast, disease or at least 2 dahan" )]
	public Task Level1( GameCtx ctx ) {

		// Each player 
		return Cmd.EachSpirit(
			// removes 1 explorer
			Cmd.RemoveExplorers(1)
				// from a land with beast, disease or at least 2 dahan
				.From( HasBeastDiseaseOr2Dahan, "land with beast, disease or at last 2 dahan" )

		).Execute( ctx );

	}

	[FearLevel( 2, "Each player removes 1 explorer/town from a land with beast, disease or at least 2 dahan" )]
	public Task Level2( GameCtx ctx ) {

		// Each player 
		return Cmd.EachSpirit( 
			// removes 1 explorer/town 
			Cmd.RemoveExplorersOrTowns(1)
				// from a land with beast, disease or at least 2 dahan
				.From(HasBeastDiseaseOr2Dahan, "land with beast, disease or at last 2 dahan")
		).Execute( ctx );

	}

	[FearLevel( 3, "Each player removes up to 4 health worth of invaders from a land with beast, disease or at least 2 dahan" )]
	public Task Level3( GameCtx ctx ) {

		// Each player 
		return Cmd.EachSpirit( 
			// removes up to 4 health worth of invaders
			Cmd.RemoveUpToNHealthOfInvaders(4)
				// from a land with beast, disease or at least 2 dahan
				.From( HasBeastDiseaseOr2Dahan, "land with beast, disease or at last 2 dahan" )
		).Execute( ctx );

	}

	static bool HasBeastDiseaseOr2Dahan( TargetSpaceCtx spaceCtx ) {
		var tokens = spaceCtx.Tokens;
		return tokens.Beasts.Any
			|| tokens.Disease.Any
			|| 2 <= tokens.Dahan.CountAll;
	}

}