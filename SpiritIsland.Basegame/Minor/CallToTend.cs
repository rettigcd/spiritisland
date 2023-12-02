namespace SpiritIsland.Basegame;

public class CallToTend {

	[MinorCard("Call to Tend",1,Element.Water,Element.Plant,Element.Animal),Slow,FromPresence(1,Filter.Dahan)]
	[Instructions( "Remove 1 Blight. -or- Push up to 3 Dahan." ), Artist( Artists.LoicBelliau )]
	static public Task ActAsync(TargetSpaceCtx ctx ) {

		return ctx.SelectActionOption(
			new SpaceAction( "remove 1 blight", ctx => ctx.RemoveBlight() ).OnlyExecuteIf(ctx => ctx.Tokens.Blight.Any), // May not have blight
			new SpaceAction( "push up to 3 dahan", ctx => ctx.PushUpToNDahan( 3 ) ) // must have Dahan
		);

	}

}
