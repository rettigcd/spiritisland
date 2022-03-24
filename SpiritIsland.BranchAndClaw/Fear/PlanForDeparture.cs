namespace SpiritIsland.BranchAndClaw;

public class PlanForDeparture : IFearOptions {

	public const string Name = "Plan for Departure";
	string IFearOptions.Name => Name;

	[FearLevel( 1, "Each player may gather 1 town into a costal land." )]
	public async Task Level1( FearCtx ctx ) {

		// Each player may gather 1 town into a costal land.
		await Cmd.EachSpirit( Cause.Fear,
			Cmd.GatherUpToNInvaders( 1, Invader.Town )
				.To( staceCtx => staceCtx.IsCoastal, "coastal land" )
		).Execute( ctx.GameState );

	}

	[FearLevel( 2, "Each player may gather 1 explroer / town into a costal land.  Defend 2 in all costal lands." )]
	public async Task Level2( FearCtx ctx ) {

		// Each player may gather 1 explorer / town into a costal land.
		await Cmd.EachSpirit( Cause.Fear,
			Cmd.GatherUpToNInvaders( 1, Invader.Explorer, Invader.Town )
				.To( staceCtx => staceCtx.IsCoastal, "coastal land" )
		).Execute( ctx.GameState );

		// Defend 2 in all costal lands.
		DefendCostal( ctx, 2 );
	}

	[FearLevel( 3, "Each player may gather 2 explorer / town into a costal land.  Defend 4 in all costal lands" )]
	public async Task Level3( FearCtx ctx ) {

		// Each player may gather 2 explorer / town into a costal land.
		await Cmd.EachSpirit( Cause.Fear,
			Cmd.GatherUpToNInvaders( 2, Invader.Explorer, Invader.Town )
				.To( staceCtx => staceCtx.IsCoastal, "coastal land" )
		).Execute( ctx.GameState );

		// Defend 4 in all costal lands
		DefendCostal( ctx, 4 );
	}

	static void DefendCostal( FearCtx ctx, int defense ) {
		var coastal = ctx.GameState.Island.AllSpaces.Where( x => x.IsCoastal ).ToArray();
		foreach(var land in coastal)
			ctx.GameState.Tokens[land].Defend.Add( defense );
	}


}