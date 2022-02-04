namespace SpiritIsland.BranchAndClaw;

public class AbsorbCorruption {

	[MinorCard( "Absorb Corruption", 1, Element.Sun, Element.Earth, Element.Plant )]
	[Slow]
	[FromPresence( 0 )]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {

		static bool CanRemoveBlight(TargetSpaceCtx ctx ) => ctx.Blight.Any && 1 <= ctx.Self.Energy;

		var gatherBlight = new SpaceAction( "Gather 1 blight", ctx => ctx.Gather( 1, TokenType.Blight ) );
		var removeBlight = new SpaceAction( "Pay 1 Energy to remove 1 blight",  Pay1EnergyToRemoveBlight )
			.Matches( CanRemoveBlight );

		var doBoth = new SpaceAction( "Do Both", 
			async ctx => { 
				await gatherBlight.Execute(ctx); 
				await removeBlight.Execute(ctx);
			}
		).FilterOption( await ctx.YouHave("2 plant") );

		await ctx.SelectActionOption( gatherBlight, removeBlight, doBoth );

	}

	static void Pay1EnergyToRemoveBlight( TargetSpaceCtx ctx ) {
		ctx.RemoveBlight(); // !!! put in safe guard that we don't just do this: ctx.Blight.Count--   because that doesn't put it back on the card.
		ctx.Self.Energy--;
	}

}