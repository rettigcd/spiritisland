namespace SpiritIsland.Basegame;

[InnatePower("All Enveloping Green"),Fast]
[FromSacredSite(1)]
public class AllEnvelopingGreen {

	[InnateTier("1 water,3 plant","Defend 2.")]
	static public Task Option1Async( TargetSpaceCtx ctx ) {
		//defend 2
		ctx.Defend(2);
		return Task.CompletedTask;
	}

	[InnateTier( "2 water,4 plant","Instead, Defend 4." )]
	static public Task Option2Async( TargetSpaceCtx ctx ) {
		//defend 4 (instead)
		ctx.Defend(4);
		return Task.CompletedTask;
	}

	[InnateTier( "3 water,5 plant,1 earth", "Also, remove 1 blight." )]
	static public Task Option3Async( TargetSpaceCtx ctx ) {
		Option2Async(ctx);
		// also remove 1 blight
		return ctx.RemoveBlight();
	}

}