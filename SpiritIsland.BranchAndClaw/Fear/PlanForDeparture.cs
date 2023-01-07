namespace SpiritIsland.BranchAndClaw;

public class PlanForDeparture : FearCardBase, IFearCard {

	public const string Name = "Plan for Departure";
	public string Text => Name;

	[FearLevel( 1, "Each player may gather 1 town into a costal land." )]
	public async Task Level1( GameCtx ctx ) {

		// Each player may gather 1 town into a costal land.
		await Cmd.EachSpirit( 
			Cmd.GatherUpToNInvaders( 1, Invader.Town )
				.To( staceCtx => staceCtx.IsCoastal, "coastal land" )
		).Execute( ctx );

	}

	[FearLevel( 2, "Each player may gather 1 explroer / town into a costal land.  Defend 2 in all costal lands." )]
	public async Task Level2( GameCtx ctx ) {

		// Each player may gather 1 explorer / town into a costal land.
		await Cmd.EachSpirit( 
			Cmd.GatherUpToNInvaders( 1, Invader.Explorer_Town )
				.To( staceCtx => staceCtx.IsCoastal, "coastal land" )
		).Execute( ctx );

		// Defend 2 in all costal lands.
		DefendCostal( ctx, 2 );
	}

	[FearLevel( 3, "Each player may gather 2 explorer / town into a costal land.  Defend 4 in all costal lands" )]
	public async Task Level3( GameCtx ctx ) {

		// Each player may gather 2 explorer / town into a costal land.
		await Cmd.EachSpirit( 
			Cmd.GatherUpToNInvaders( 2, Invader.Explorer_Town )
				.To( staceCtx => staceCtx.IsCoastal, "coastal land" )
		).Execute( ctx );

		// Defend 4 in all costal lands
		DefendCostal( ctx, 4 );
	}

	static void DefendCostal( GameCtx ctx, int defense ) {
		var coastal = ctx.GameState.AllActiveSpaces
			.Where( x => x.Space.IsCoastal )
			.ToArray();
		foreach(var land in coastal)
			land.Defend.Add( defense );
	}


}