namespace SpiritIsland.FeatherAndFlame;

[InnatePower("Water Nourishes Life's Growth"), Fast]
[FromPresence(0)]
internal class WaterNourishesLifesGrowth {

	[InnateOption( "3 water,2 plant", "Gain 1 Energy. You may remove 1 Blight by removing one of your Presence (From target land).", 0)]
	static public async Task Option1( TargetSpaceCtx ctx ) {
		// Gain 1 Energy.
		++ctx.Self.Energy;

		// You may remove 1 Blight by removing one of your Presence (From target land).
		if(ctx.Blight.Any && await ctx.Self.UserSelectsFirstText("Destory 1 presence to remove 1 blight?","Yes, remove blight and presence", "No, thank you" )) {
			await ctx.Tokens.Destroy( ctx.Self.Token, 1 );
			await ctx.Blight.Remove(1);
		}
	}

	[InnateOption( "5 water,1 earth,2 plant", "Gain +1 Energy. Gather up to 1 Dahan.", 1 )]
	static public Task Option2( TargetSpaceCtx ctx ) {
		// Gain +1 Energy.
		++ctx.Self.Energy;

		// Gather up to 1 Dahan.
		return ctx.GatherUpToNDahan(1);
	}

	[InnateOption( "7 water,2 earth,3 plant", "When Blight would be added to target land, instead leave it on the card.", 2 )]
	static public Task Option3( TargetSpaceCtx ctx ) {
		// When Blight would be added to target land, instead leave it on the card.
		ctx.Blight.Block(); // auto-cleared at end of round
		return Task.CompletedTask;
	}

}

