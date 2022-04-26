namespace SpiritIsland.BranchAndClaw;

class DepartTheDangerousLand : IFearOptions {

	public const string Name = "Depart the Dangerous Land";
	string IFearOptions.Name => Name;

	[FearLevel( 1, "Each player removes 1 explorer from a land with beast, disease or at least 2 dahan" )]
	public Task Level1( FearCtx ctx ) {

		// Each player 
		return Cmd.EachSpirit(
			// removes 1 explorer
			Cmd.RemoveExplorers(1)
				// from a land with beast, disease or at least 2 dahan
				.From( HasBeastDiseaseOr2Dahan, "land with beast, disease or at last 2 dahan" )

		).Execute( ctx.GameState );

	}

	[FearLevel( 2, "Each player removes 1 explorer/town from a land with beast, disease or at least 2 dahan" )]
	public Task Level2( FearCtx ctx ) {

		// Each player 
		return Cmd.EachSpirit( 
			// removes 1 explorer/town 
			Cmd.RemoveExplorersOrTowns(1)
				// from a land with beast, disease or at least 2 dahan
				.From(HasBeastDiseaseOr2Dahan, "land with beast, disease or at last 2 dahan")
		).Execute( ctx.GameState );

	}

	[FearLevel( 3, "Each player removes up to 4 health worth of invaders from a land with beast, disease or at least 2 dahan" )]
	public Task Level3( FearCtx ctx ) {

		// Each player 
		return Cmd.EachSpirit( 
			// removes up to 4 health worth of invaders
			Cmd.RemoveUpToNHealthOfInvaders(4)
				// from a land with beast, disease or at least 2 dahan
				.From( HasBeastDiseaseOr2Dahan, "land with beast, disease or at last 2 dahan" )
		).Execute( ctx.GameState );

	}

	static bool HasBeastDiseaseOr2Dahan( TargetSpaceCtx spaceCtx ) {
		var tokens = spaceCtx.Tokens;
		return tokens.Beasts.Any
			|| tokens.Disease.Any
			|| 2 <= tokens.Dahan.Count;
	}

}