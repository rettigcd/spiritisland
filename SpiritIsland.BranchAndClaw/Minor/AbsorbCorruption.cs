namespace SpiritIsland.BranchAndClaw;

public class AbsorbCorruption {

	[MinorCard( "Absorb Corruption", 1, Element.Sun, Element.Earth, Element.Plant ),Slow,FromPresence( 0 )]
	[Instructions( "Gather 1 Blight. -or- Pay 1 Energy to remove 1 Blight. -If you have- 2 Plant: You may do both." ), Artist( Artists.NolanNasser )]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {

		static bool CanRemoveBlight(TargetSpaceCtx ctx ) => ctx.Blight.Any && 1 <= ctx.Self.Energy;

		var gatherBlight = new SpaceCmd( "Gather 1 blight", ctx => ctx.Gather( 1, Token.Blight ) );
		var removeBlight = new SpaceCmd( "Pay 1 Energy to remove 1 blight",  Pay1EnergyToRemoveBlight )
			.OnlyExecuteIf( CanRemoveBlight );

		var doBoth = new SpaceCmd( "Do Both", 
			async ctx => { 
				await gatherBlight.ActAsync(ctx); 
				await removeBlight.ActAsync(ctx);
			}
		).OnlyExecuteIf( await ctx.YouHave("2 plant") );

		await ctx.SelectActionOption( gatherBlight, removeBlight, doBoth );

	}

	static void Pay1EnergyToRemoveBlight( TargetSpaceCtx ctx ) {
		ctx.RemoveBlight();
		ctx.Self.Energy--;
	}

}